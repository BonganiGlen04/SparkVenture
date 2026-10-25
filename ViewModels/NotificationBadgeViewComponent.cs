
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.ViewModels;

namespace SparkVenture.ViewComponents;

public class NotificationBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public NotificationBadgeViewComponent(
        ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync(
        bool dotOnly = false)
    {
        int count = 0;

        var userIdValue = UserClaimsPrincipal
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdValue, out Guid userId))
        {
            count = await _db.Notifications
                .AsNoTracking()
                .CountAsync(n =>
                    n.UserId == userId &&
                    n.Channel == "InApp" &&
                    n.ReadAtUtc == null);
        }

        return View(
            new NotificationBadgeViewModel(count, dotOnly));
    }
}
