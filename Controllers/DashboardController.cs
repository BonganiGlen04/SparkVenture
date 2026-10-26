using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.ViewModels;
using SparkVenture.Data;   // Change if ApplicationDbContext is in a different namespace.
using SparkVenture.Models; // Change if ApplicationUser is in a different namespace.

namespace SparkVenture.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // /Dashboard or /Dashboard/Index?businessId=...
    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId)
    {

        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (!businessId.HasValue &&
        HttpContext.Items[
            SparkVenture.Infrastructure
                .ActiveBusinessContextMiddleware.BusinessIdItemKey
        ] is Guid activeBusinessId)
        {
            return RedirectToAction(nameof(Index),
                new { businessId = activeBusinessId });
        }

        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        // Business selector: don't silently choose one when the user has several.
        if (!businessId.HasValue)
        {
            var choices = await _db.BusinessMembers.AsNoTracking()
                .Where(m => m.UserId == userId && m.MembershipStatus == "Active")
                .Select(m => m.BusinessId)
                .Distinct()
                .Take(2)
                .ToListAsync();

            if (choices.Count == 0)
                return RedirectToAction("Create", "Businesses");

            if (choices.Count > 1)
                return RedirectToAction("Index", "Businesses");

            return RedirectToAction(nameof(Index), new { businessId = choices[0] });
        }

        if (businessId.Value == Guid.Empty)
            return NotFound();

        // Object-level authorisation. Never trust an ID supplied in the URL.
        var selected = await (
            from m in _db.BusinessMembers.AsNoTracking()
            join b in _db.Businesses.AsNoTracking()
                on m.BusinessId equals b.BusinessId
            where m.UserId == userId
                && m.MembershipStatus == "Active"
                && m.BusinessId == businessId.Value
            select new
            {
                b.BusinessId,
                b.TradingName,
                b.RegistrationNumber,
                b.SectorId,
                b.ProvinceId,
                b.BusinessStageId,
                m.BusinessRole
            }).FirstOrDefaultAsync();

        if (selected == null)
            return NotFound();

        var id = selected.BusinessId;
        var todayUtc = DateTime.UtcNow;
        var currentMonthStart = new DateTime(todayUtc.Year, todayUtc.Month, 1);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var firstChartMonthStart = currentMonthStart.AddMonths(-5);

        var sector = await _db.Sectors.AsNoTracking()
            .Where(s => s.SectorId == selected.SectorId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync() ?? "Not specified";

        var province = await _db.ReferenceValues.AsNoTracking()
            .Where(r => r.ReferenceValueId == selected.ProvinceId && r.Category == "Province")
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? "Not specified";

        var businessStage = await _db.ReferenceValues.AsNoTracking()
            .Where(r => r.ReferenceValueId == selected.BusinessStageId && r.Category == "BusinessStage")
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? "Not specified";

        // Only POSTED cashbook entries count. This excludes reversed/void entries.
        // Group by month, year and direction; SQL Server executes the aggregation.
        var cashflowRows = await _db.CashbookEntries.AsNoTracking()
            .Where(e => e.BusinessId == id
                        && e.Status == "Posted"
                        && e.EntryType != "Reversal"
                        && e.EntryDate >= firstChartMonthStart
                        && e.EntryDate < nextMonthStart)
            .GroupBy(e => new { e.EntryDate.Year, e.EntryDate.Month, e.Direction })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Direction,
                Total = g.Sum(e => e.Amount)
            })
            .ToListAsync();

        var cashflowMonths = Enumerable.Range(0, 6)
            .Select(offset => currentMonthStart.AddMonths(offset - 5))
            .Select(month => new DashboardMonthViewModel
            {
                Label = month.ToString("MMM"),
                Income = cashflowRows
                    .Where(r => r.Year == month.Year && r.Month == month.Month && r.Direction == "In")
                    .Sum(r => r.Total),
                Expenses = cashflowRows
                    .Where(r => r.Year == month.Year && r.Month == month.Month && r.Direction == "Out")
                    .Sum(r => r.Total)
            }).ToList();

        var chartMaximum = Math.Max(1m, cashflowMonths.Max(x => Math.Max(x.Income, x.Expenses)));
        foreach (var month in cashflowMonths)
        {
            month.IncomeBarPercent = (int)Math.Round(month.Income / chartMaximum * 100m);
            month.ExpensesBarPercent = (int)Math.Round(month.Expenses / chartMaximum * 100m);
        }

        var openInvoicesQuery = _db.Invoices.AsNoTracking()
            .Where(i => i.BusinessId == id
                        && i.Status != "Draft"
                        && i.Status != "Cancelled"
                        && i.Status != "Void"
                        && i.TotalAmount > i.AmountPaid);

        var openInvoiceCount = await openInvoicesQuery.CountAsync();
        var outstandingInvoices = await openInvoicesQuery
            .SumAsync(i => (decimal?)(i.TotalAmount - i.AmountPaid)) ?? 0m;

        var savedOpportunitiesCount = await _db.SavedOpportunities.AsNoTracking()
            .CountAsync(s => s.BusinessId == id);

        var unreadNotificationsCount = await _db.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == userId
                             && n.BusinessId == id
                             && n.Channel == "InApp"
                             && n.ReadAtUtc == null);

        var latestRegistrationStatus = await _db.RegistrationApplications.AsNoTracking()
            .Where(a => a.BusinessId == id)
            .OrderByDescending(a => a.UpdatedAtUtc)
            .Select(a => a.Status)
            .FirstOrDefaultAsync();

        var hasPostedCashbookEntries = await _db.CashbookEntries.AsNoTracking()
            .AnyAsync(e => e.BusinessId == id && e.Status == "Posted");

        // Activity is real user/business data: notifications and cashbook entries.
        var latestNotifications = await _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId && n.BusinessId == id && n.Channel == "InApp")
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(5)
            .Select(n => new { n.Title, n.CreatedAtUtc })
            .ToListAsync();

        var latestEntries = await _db.CashbookEntries.AsNoTracking()
            .Where(e => e.BusinessId == id && e.Status == "Posted")
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(5)
            .Select(e => new { e.Description, e.Direction, e.Amount, e.CreatedAtUtc })
            .ToListAsync();

        var activities = latestNotifications.Select(n => new DashboardActivityViewModel
        {
            Title = n.Title,
            Detail = "Notification",
            OccurredAtUtc = n.CreatedAtUtc,
            Icon = "🔔",
            Kind = "notification",
            IconBgClass = "bg-indigo-100",
            IconTextClass = "text-indigo-700",
            Controller = "Notifications",
            Action = "Index"
        }).Concat(latestEntries.Select(e => new DashboardActivityViewModel
        {
            Title = e.Description,
            Detail = $"{(e.Direction == "In" ? "Income" : "Expense")}: R {e.Amount:N2}",
            OccurredAtUtc = e.CreatedAtUtc,
            Icon = e.Direction == "In" ? "↗" : "↘",
            Kind = e.Direction == "In" ? "income" : "expense",
            IconBgClass = e.Direction == "In" ? "bg-emerald-100" : "bg-amber-100",
            IconTextClass = e.Direction == "In" ? "text-emerald-700" : "text-amber-700",
            Controller = "Cashbook",
            Action = "Index"
        }))
        .OrderByDescending(a => a.OccurredAtUtc)
        .Take(5)
        .ToList();

        bool hasRegistrationNumber = !string.IsNullOrWhiteSpace(selected.RegistrationNumber);
        bool hasExploredRegistration = hasRegistrationNumber || latestRegistrationStatus != null;

        var model = new DashboardViewModel
        {
            BusinessId = id,
            Email = _userManager.GetUserName(User) ?? string.Empty,
            BusinessName = selected.TradingName,
            BusinessRole = selected.BusinessRole,
            Sector = sector,
            Province = province,
            BusinessStage = businessStage,
            IncomeThisMonth = cashflowMonths[^1].Income,
            ExpensesThisMonth = cashflowMonths[^1].Expenses,
            OpenInvoiceCount = openInvoiceCount,
            OutstandingInvoices = outstandingInvoices,
            SavedOpportunitiesCount = savedOpportunitiesCount,
            UnreadNotificationsCount = unreadNotificationsCount,
            RegistrationPreparationStatus = latestRegistrationStatus,
            HasCipcRegistrationNumber = hasRegistrationNumber,
            CashflowMonths = cashflowMonths,
            RecentActivity = activities,
            GettingStartedTasks = new()
            {
                new DashboardTaskViewModel
                {
                    Title = "Create a business profile",
                    Description = "Your business now has a SparkVenture workspace.",
                    IsComplete = true,
                    Controller = "Businesses",
                    Action = "Details",
                    UsesBusinessIdAsId = true
                },
                new DashboardTaskViewModel
                {
                    Title = "Record your first transaction",
                    Description = "Add an income or expense entry to your cashbook.",
                    IsComplete = hasPostedCashbookEntries,
                    Controller = "Cashbook",
                    Action = "Create"
                },
                new DashboardTaskViewModel
                {
                    Title = "Save an opportunity",
                    Description = "Explore tenders, funding and market opportunities.",
                    IsComplete = savedOpportunitiesCount > 0,
                    Controller = "Opportunities",
                    Action = "Index"
                },
                new DashboardTaskViewModel
                {
                    Title = "Explore business formalisation",
                    Description = "Keep your CIPC details or registration-preparation progress up to date, where applicable.",
                    IsComplete = hasExploredRegistration,
                    Controller = "RegistrationApplications",
                    Action = "Index"
                }
            }
        };

        // Optional shared-layout improvement: preserve the selected business in sidebar links.
        ViewData["CurrentBusinessId"] = id;
        return View(model);
    }
}
