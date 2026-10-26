using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.Services;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class RecommendationsController : OpportunityControllerBase
{
    public RecommendationsController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        if (businessId is null) {
            var id = await SingleBusinessAsync(userId, ct);
            return id is Guid found ? RedirectToAction(nameof(Index), new { businessId = found }) : ChooseBusiness();
        }
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var recommendations = await OpportunityMatchingService.CalculateAsync(Db, businessId.Value, ct);
        var last = await Db.RecommendationRuns.AsNoTracking()
            .Where(x => x.BusinessId == businessId.Value && x.IsCurrent)
            .OrderByDescending(x => x.CreatedAtUtc).Select(x => (DateTime?)x.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
        return View(new RecommendationsIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name,
            CanManage = access.CanManage, Matches = recommendations,
            LastSavedRunUtc = last
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSnapshot(Guid businessId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        var matches = await OpportunityMatchingService.CalculateAsync(Db, businessId, ct);
        var opportunityIds = matches.Select(m => m.Opportunity.OpportunityId).ToList();
        var revisionMap = await Db.Opportunities.AsNoTracking()
            .Where(x => opportunityIds.Contains(x.OpportunityId))
            .Select(x => new { x.OpportunityId, x.Revision }).ToDictionaryAsync(x => x.OpportunityId, x => x.Revision, ct);
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var previous = await Db.RecommendationRuns
            .Where(x => x.BusinessId == businessId && x.IsCurrent).ToListAsync(ct);
        foreach (var item in previous) item.IsCurrent = false;
        var runId = Guid.NewGuid();
        Db.RecommendationRuns.Add(new RecommendationRun {
            RecommendationRunId = runId, BusinessId = businessId, TriggeredByUserId = userId,
            RunType = "OnDemand", AlgorithmVersion = "rules-v1", IsCurrent = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        foreach (var item in matches) {
            Db.OpportunityMatches.Add(new OpportunityMatch {
                OpportunityMatchId = Guid.NewGuid(), RecommendationRunId = runId,
                BusinessId = businessId, OpportunityId = item.Opportunity.OpportunityId,
                OpportunityRevision = revisionMap[item.Opportunity.OpportunityId], Score = item.IndicativeScore,
                MatchBand = item.IndicativeScore >= 80 ? "High" : item.IndicativeScore >= 50 ? "Medium" : "Low",
                MandatoryCriteriaPassed = item.MandatoryCriteriaPassed,
                Explanation = item.Explanation,
                BoostExplanation = item.Gaps.Count == 0 ? null : string.Join("; ", item.Gaps).Substring(0, Math.Min(190, string.Join("; ", item.Gaps).Length)),
                AlgorithmVersion = "rules-v1"
            });
        }
        Audit(userId, businessId, "recommendations.generated", "RecommendationRun", runId);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "An explainable recommendation snapshot was saved.";
        return RedirectToAction(nameof(Index), new { businessId });
    }
}
