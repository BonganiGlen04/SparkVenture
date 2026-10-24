using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.ViewModels;

namespace SparkVenture.ViewComponents;

/// <summary>
/// Renders the unread-notification badge (sidebar number or top-bar dot).
/// Usage: @await Component.InvokeAsync("NotificationBadge", new { dotOnly = false })
/// </summary>
public class NotificationBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public NotificationBadgeViewComponent(ApplicationDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(bool dotOnly = false)
    {
        var count = 0;

        if (Guid.TryParse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            // TODO: match your Notification entity (owner + read flag property names).
            // count = await _db.Notifications
            //     .AsNoTracking()
            //     .CountAsync(n => n.UserId == userId && !n.IsRead);
            await Task.CompletedTask;
            count = 3; // demo value - remove when the query above is wired up
        }

        return View(new NotificationBadgeViewModel(count, dotOnly));
    }
}
