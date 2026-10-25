using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class NotificationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public NotificationsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(_users.GetUserId(User), out userId);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var query = _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId && n.Channel == "InApp");
        var unreadCount = await query.CountAsync(n => n.ReadAtUtc == null,
            cancellationToken);
        var items = await query.OrderByDescending(n => n.CreatedAtUtc).Take(100)
            .Select(n => new NotificationRowViewModel
            {
                NotificationId = n.NotificationId,
                BusinessId = n.BusinessId,
                Title = n.Title,
                Body = n.Body,
                Channel = n.Channel,
                CreatedAtUtc = n.CreatedAtUtc,
                ReadAtUtc = n.ReadAtUtc
            }).ToListAsync(cancellationToken);
        return View(new NotificationsIndexViewModel
        {
            UnreadCount = unreadCount, Items = items
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var item = await _db.Notifications.SingleOrDefaultAsync(n =>
            n.NotificationId == notificationId && n.UserId == userId &&
            n.Channel == "InApp", cancellationToken);
        if (item is null) return NotFound();
        if (item.ReadAtUtc is null)
        {
            item.ReadAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var unread = await _db.Notifications.Where(n => n.UserId == userId &&
            n.Channel == "InApp" && n.ReadAtUtc == null)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var notification in unread) notification.ReadAtUtc = now;
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Preferences(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var rows = await _db.NotificationPreferences.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Channel).ThenBy(x => x.Category)
            .Select(x => new NotificationPreferenceRowViewModel
            {
                NotificationPreferenceId = x.NotificationPreferenceId,
                Channel = x.Channel, Category = x.Category,
                IsEnabled = x.IsEnabled
            }).ToListAsync(cancellationToken);
        return View(new NotificationPreferencesViewModel { Items = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Preferences(Guid[]? enabledIds,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        var enabled = new HashSet<Guid>(enabledIds ?? Array.Empty<Guid>());
        var rows = await _db.NotificationPreferences
            .Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var preference in rows)
        {
            var next = enabled.Contains(preference.NotificationPreferenceId);
            if (preference.IsEnabled == next) continue;
            preference.IsEnabled = next;
            preference.UpdatedAtUtc = now;
        }
        await _db.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = "Notification preferences saved.";
        return RedirectToAction(nameof(Preferences));
    }


    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var userIdValue = _users.GetUserId(User);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Challenge();

        int count = await _db.Notifications
            .AsNoTracking()
            .CountAsync(n =>
                n.UserId == userId &&
                n.Channel == "InApp" &&
                n.ReadAtUtc == null);

        Response.Headers.CacheControl = "no-store";

        return Json(new { count });
    }

}
