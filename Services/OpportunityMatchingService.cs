using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.ViewModels;

namespace SparkVenture.Services;

// Deterministic, explanatory rule-based suggestions. NOT legal eligibility decisions.
// Unknown rules never count as passed. Scores measure known profile signals only.
public static class OpportunityMatchingService
{
    private static readonly string[] StageOrder = { "IDEA", "STARTUP", "EARLY", "GROWTH", "ESTABLISHED" };

    public static async Task<List<RecommendationVm>> CalculateAsync(
        ApplicationDbContext db, Guid businessId, CancellationToken ct)
    {
        var business = await db.Businesses.AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .Select(x => new { x.SectorId, x.ProvinceId, x.BusinessStageId })
            .SingleOrDefaultAsync(ct);
        if (business is null) return new();
        var sector = await db.Sectors.AsNoTracking()
            .Where(x => x.SectorId == business.SectorId).Select(x => x.Code)
            .FirstOrDefaultAsync(ct);
        var references = await db.ReferenceValues.AsNoTracking()
            .Where(x => x.ReferenceValueId == business.ProvinceId ||
                        x.ReferenceValueId == business.BusinessStageId)
            .Select(x => new { x.ReferenceValueId, x.Code }).ToListAsync(ct);
        var province = references.FirstOrDefault(x => x.ReferenceValueId == business.ProvinceId)?.Code;
        var stage = references.FirstOrDefault(x => x.ReferenceValueId == business.BusinessStageId)?.Code;

        // Only verified, unexpired credentials are considered present.
        var credentialCodes = await (
            from credential in db.BusinessCredentials.AsNoTracking()
            join type in db.ReferenceValues.AsNoTracking()
                on credential.CredentialTypeId equals type.ReferenceValueId
            where credential.BusinessId == businessId && credential.Status == "Verified" &&
                  (credential.ExpiresOn == null || credential.ExpiresOn >= DateTime.UtcNow.Date)
            select type.Code).ToListAsync(ct);
        var credentials = credentialCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        // A metadata-only seeded document is not usable evidence. Only count
        // a same-business document when its binary content is really stored.
        var documentCodes = await (
            from link in db.BusinessDocuments.AsNoTracking()
            join document in db.Documents.AsNoTracking()
                on link.DocumentId equals document.DocumentId
            where link.BusinessId == businessId && document.BusinessId == businessId &&
                  db.DocumentContents.Any(c => c.DocumentId == document.DocumentId)
            select link.DocumentPurpose).ToListAsync(ct);
        var documentPurposes = documentCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cards = await OpportunityQueries.CardsAsync(db, businessId, ct);
        var open = cards.Where(x => x.IsOpen).ToList();
        var ids = open.Select(x => x.OpportunityId).ToList();
        var requirements = await db.OpportunityRequirements.AsNoTracking()
            .Where(x => ids.Contains(x.OpportunityId)).ToListAsync(ct);
        var opportunities = await db.Opportunities.AsNoTracking()
            .Where(x => ids.Contains(x.OpportunityId))
            .Select(x => new { x.OpportunityId, x.MinimumStageId, x.MaximumStageId })
            .ToListAsync(ct);
        var stageLookup = await db.ReferenceValues.AsNoTracking()
            .Where(x => x.Category == "BusinessStage")
            .Select(x => new { x.ReferenceValueId, x.Code }).ToListAsync(ct);
        var output = new List<RecommendationVm>();
        foreach (var card in open)
        {
            var checks = requirements.Where(x => x.OpportunityId == card.OpportunityId).ToList();
            decimal knownWeight = 0m, earnedWeight = 0m;
            int unknown = 0;
            bool mandatoryFailed = false;
            var gaps = new List<string>();
            foreach (var req in checks)
            {
                string? actual = req.RequirementType switch {
                    "Sector" => sector, "Province" => province, "BusinessStage" => stage,
                    _ => null
                };
                bool? passed = null;
                if (req.RequirementType is "Sector" or "Province" or "BusinessStage")
                {
                    if (!string.IsNullOrWhiteSpace(actual))
                        passed = Matches(actual, req.Value, req.Operator);
                }
                else if (req.RequirementType == "Credential")
                    passed = credentials.Contains(req.Value);
                else if (req.RequirementType == "Document")
                    passed = documentPurposes.Contains(req.Value);
                // Age, employee count, revenue etc. need additional verified data.
                if (passed is null) {
                    unknown++;
                    if (req.IsMandatory) { mandatoryFailed = true; gaps.Add(req.RequirementType + ": needs checking"); }
                } else {
                    var weight = Math.Max(0m, Convert.ToDecimal(req.Weight));
                    knownWeight += weight;
                    if (passed.Value) earnedWeight += weight;
                    else {
                        if (req.IsMandatory) mandatoryFailed = true;
                        gaps.Add(req.RequirementType + ": check " + req.Value);
                    }
                }
            }
            var stageRule = opportunities.FirstOrDefault(x => x.OpportunityId == card.OpportunityId);
            var min = stageRule is null ? null :
                stageLookup.FirstOrDefault(x => x.ReferenceValueId == stageRule.MinimumStageId)?.Code;
            var max = stageRule is null ? null :
                stageLookup.FirstOrDefault(x => x.ReferenceValueId == stageRule.MaximumStageId)?.Code;
            if (stageRule is not null && stage is not null && min is not null && max is not null)
            {
                var index = Array.IndexOf(StageOrder, stage);
                var low = Array.IndexOf(StageOrder, min);
                var high = Array.IndexOf(StageOrder, max);
                if (index >= 0 && low >= 0 && high >= 0) {
                    knownWeight += 20m;
                    if (index >= low && index <= high) earnedWeight += 20m;
                    else { mandatoryFailed = true; gaps.Add("Business stage is outside the opportunity range"); }
                } else { unknown++; mandatoryFailed = true; }
            }
            else { unknown++; mandatoryFailed = true; }
            var score = knownWeight > 0 ? Math.Round(earnedWeight / knownWeight * 100m, 0) : 0m;
            var label = mandatoryFailed ? "Needs checks / may not qualify" :
                unknown > 0 ? "Potential match – verify details" : "Known checks match";
            output.Add(new RecommendationVm {
                Opportunity = card, IndicativeScore = score, Readiness = label,
                MandatoryCriteriaPassed = !mandatoryFailed,
                UnknownChecks = unknown, Gaps = gaps.Distinct().Take(5).ToList(),
                Explanation = $"{earnedWeight:0}/{knownWeight:0} known-rule weight matched; {unknown} checks need verification. This is not an eligibility decision."
            });
        }
        return output.OrderByDescending(x => x.MandatoryCriteriaPassed)
            .ThenByDescending(x => x.IndicativeScore)
            .ThenBy(x => x.Opportunity.ClosesAtUtc).Take(100).ToList();
    }

    private static bool Matches(string actual, string expected, string op)
    {
        if (op == "Equals") return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        if (op == "In") return (expected ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Contains(actual, StringComparer.OrdinalIgnoreCase);
        return false;
    }
}
