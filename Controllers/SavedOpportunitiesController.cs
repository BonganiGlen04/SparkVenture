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
public sealed class SavedOpportunitiesController : OpportunityControllerBase
{
    public SavedOpportunitiesController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

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
        var cards = (await OpportunityQueries.CardsAsync(Db, businessId.Value, ct))
            .ToDictionary(x => x.OpportunityId);
        var saved = await Db.SavedOpportunities.AsNoTracking()
            .Where(x => x.BusinessId == businessId.Value).OrderByDescending(x => x.SavedAtUtc)
            .ToListAsync(ct);
        return View(new SavedOpportunitiesIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name, CanManage = access.CanManage,
            Items = saved.Where(x => cards.ContainsKey(x.OpportunityId))
                .Select(x => new SavedOpportunityRowVm {
                    SavedOpportunityId = x.SavedOpportunityId, Notes = x.Notes,
                    Opportunity = cards[x.OpportunityId]
                }).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Guid businessId, Guid opportunityId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (!await Db.Opportunities.AnyAsync(x => x.OpportunityId == opportunityId, ct)) return NotFound();
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        if (!await Db.SavedOpportunities.AnyAsync(x => x.BusinessId == businessId && x.OpportunityId == opportunityId, ct)) {
            var id = Guid.NewGuid();
            Db.SavedOpportunities.Add(new SavedOpportunity {
                SavedOpportunityId = id, BusinessId = businessId, OpportunityId = opportunityId,
                SavedByUserId = userId, SavedAtUtc = DateTime.UtcNow
            });
            Audit(userId, businessId, "opportunity.saved", "SavedOpportunity", id);
            await Db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Opportunity saved.";
        return RedirectToAction("Details", "Opportunities", new { businessId, id = opportunityId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid businessId, Guid savedOpportunityId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        var item = await Db.SavedOpportunities.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.SavedOpportunityId == savedOpportunityId, ct);
        if (item is null) return NotFound();
        Db.SavedOpportunities.Remove(item);
        Audit(userId, businessId, "opportunity.unsaved", "SavedOpportunity", savedOpportunityId);
        await Db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index), new { businessId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(Guid businessId, Guid savedOpportunityId, string? notes, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (notes?.Length > 350) return BadRequest("Notes must be 350 characters or fewer.");
        var item = await Db.SavedOpportunities.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.SavedOpportunityId == savedOpportunityId, ct);
        if (item is null) return NotFound();
        item.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        await Db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index), new { businessId });
    }
}
