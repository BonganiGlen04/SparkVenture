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
public sealed class OpportunitiesController : OpportunityControllerBase
{
    public OpportunitiesController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, string? search, string? type,
        string? state, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        if (businessId is null) {
            var choice = await SingleBusinessAsync(userId, ct);
            return choice is Guid id ? RedirectToAction(nameof(Index), new { businessId = id }) : ChooseBusiness();
        }
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var cards = await OpportunityQueries.CardsAsync(Db, businessId.Value, ct);
        var selectedState = state is "All" or "Upcoming" ? state : "Open";
        if (selectedState == "Open") cards = cards.Where(x => x.IsOpen).ToList();
        if (selectedState == "Upcoming") cards = cards.Where(x => x.Status == "Upcoming" ||
            (x.OpensAtUtc.HasValue && x.OpensAtUtc.Value > DateTime.UtcNow)).ToList();
        var chosenType = string.IsNullOrWhiteSpace(type) ? "All" : type.Trim();
        if (chosenType != "All") cards = cards.Where(x => string.Equals(x.Type, chosenType, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(search)) {
            var term = search.Trim();
            cards = cards.Where(x => x.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return View(new OpportunitiesIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name, CanManage = access.CanManage,
            Search = search ?? "", Type = chosenType, State = selectedState,
            Opportunities = cards.Take(100).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var card = (await OpportunityQueries.CardsAsync(Db, businessId, ct))
            .FirstOrDefault(x => x.OpportunityId == id);
        if (card is null) return NotFound();
        var requirements = await Db.OpportunityRequirements.AsNoTracking()
            .Where(x => x.OpportunityId == id).OrderByDescending(x => x.IsMandatory)
            .Select(x => new OpportunityRequirementVm {
                Type = x.RequirementType, Operator = x.Operator,
                Value = x.Value, IsMandatory = x.IsMandatory
            }).ToListAsync(ct);
        var tenderId = await Db.TenderApplications.AsNoTracking()
            .Where(x => x.BusinessId == businessId && x.OpportunityId == id)
            .Select(x => (Guid?)x.TenderApplicationId).FirstOrDefaultAsync(ct);
        return View(new OpportunityDetailsVm {
            BusinessId = businessId, BusinessName = access.Name, CanManage = access.CanManage,
            Opportunity = card, Requirements = requirements, TenderApplicationId = tenderId
        });
    }
}
