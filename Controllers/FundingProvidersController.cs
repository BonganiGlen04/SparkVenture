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
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class FundingProvidersController : FundingControllerBase
{
    public FundingProvidersController(ApplicationDbContext db, UserManager<ApplicationUser> users)
        : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, string? search, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!businessId.HasValue)
        {
            var selected = await GetSingleBusinessIdAsync(userId, ct);
            return selected.HasValue ? RedirectToAction(nameof(Index), new { businessId = selected.Value }) : PickBusiness();
        }
        var access = await GetAccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        search = (search ?? "").Trim();
        if (search.Length > 100) search = search.Substring(0, 100);
        var query = Db.FundingProviders.AsNoTracking().Where(x => x.IsActive);
        if (search.Length > 0) query = query.Where(x => x.Name.Contains(search) || x.ProviderType.Contains(search));
        var providers = await query.OrderBy(x => x.Name).Take(100)
            .Select(x => new FundingProviderRowVm {
                FundingProviderId = x.FundingProviderId, Name = x.Name,
                ProviderType = x.ProviderType, WebsiteUrl = x.WebsiteUrl
            }).ToListAsync(ct);
        return View(new FundingProvidersIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name,
            CanManage = access.CanManage, Search = search, Providers = providers
        });
    }
}
