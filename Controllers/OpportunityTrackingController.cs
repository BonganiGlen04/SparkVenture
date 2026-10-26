using System;
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
public sealed class OpportunityTrackingController : OpportunityControllerBase
{
    public OpportunityTrackingController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }
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
            .Where(x => x.BusinessId == businessId.Value).ToListAsync(ct);
        var items = saved.Where(x => cards.ContainsKey(x.OpportunityId))
            .Select(x => new SavedOpportunityRowVm {
                SavedOpportunityId = x.SavedOpportunityId, Notes = x.Notes,
                Opportunity = cards[x.OpportunityId]
            }).OrderBy(x => x.Opportunity.ClosesAtUtc).ToList();
        var applications = await (from app in Db.TenderApplications.AsNoTracking()
            join opportunity in Db.Opportunities.AsNoTracking()
                on app.OpportunityId equals opportunity.OpportunityId
            where app.BusinessId == businessId.Value
            orderby app.SubmissionDeadlineUtc
            select new TenderRowVm {
                TenderApplicationId = app.TenderApplicationId, OpportunityId = app.OpportunityId,
                Title = opportunity.Title, Status = app.Status,
                DeadlineUtc = app.SubmissionDeadlineUtc, SubmittedAtUtc = app.SubmittedAtUtc
            }).Take(150).ToListAsync(ct);
        var soon = DateTime.UtcNow.AddDays(14);
        var now = DateTime.UtcNow;
        return View(new OpportunityTrackingVm {
            BusinessId = businessId.Value, BusinessName = access.Name,
            Saved = items, Applications = applications,
            DueSoon = items.Count(x => x.Opportunity.IsOpen && x.Opportunity.ClosesAtUtc <= soon) +
                applications.Count(x => x.Status == "InProgress" &&
                    x.DeadlineUtc >= now && x.DeadlineUtc <= soon)
        });
    }
}
