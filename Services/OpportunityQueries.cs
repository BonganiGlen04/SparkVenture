using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.ViewModels;

namespace SparkVenture.Services;

// Query helper only: no DI registration required.
public static class OpportunityQueries
{
    public static async Task<List<OpportunityCardVm>> CardsAsync(
        ApplicationDbContext db, Guid businessId, CancellationToken ct)
    {
        var savedIds = await db.SavedOpportunities.AsNoTracking()
            .Where(x => x.BusinessId == businessId)
            .Select(x => x.OpportunityId).ToListAsync(ct);
        var saved = savedIds.ToHashSet();
        var rows = await (from opportunity in db.Opportunities.AsNoTracking()
                          join source in db.OpportunitySources.AsNoTracking()
                            on opportunity.OpportunitySourceId equals source.OpportunitySourceId
                          orderby opportunity.ClosesAtUtc, opportunity.Title
                          select new {
                              opportunity.OpportunityId, opportunity.Title, opportunity.Description,
                              opportunity.OpportunityType, opportunity.Status, opportunity.OpensAtUtc,
                              opportunity.ClosesAtUtc, opportunity.ExternalId,
                              SourceName = source.Name, source.WebsiteUrl
                          }).Take(500).ToListAsync(ct);
        return rows.Select(x => new OpportunityCardVm {
            OpportunityId = x.OpportunityId, Title = x.Title ?? "Untitled opportunity",
            Description = x.Description ?? "", Type = x.OpportunityType ?? "Other",
            Status = x.Status ?? "Unknown", OpensAtUtc = x.OpensAtUtc,
            ClosesAtUtc = x.ClosesAtUtc, SourceName = x.SourceName ?? "Source",
            SourceWebsite = x.WebsiteUrl, IsSaved = saved.Contains(x.OpportunityId),
            IsDemo = (x.ExternalId ?? "").StartsWith("SEED-", StringComparison.OrdinalIgnoreCase)
        }).ToList();
    }
    public static bool IsValidHttpsUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
