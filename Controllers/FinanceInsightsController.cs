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
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class FinanceInsightsController : FinanceControllerBase
{
    public FinanceInsightsController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        if (businessId is null)
        {
            var selected = await DefaultBusinessAsync(userId, ct);
            return selected is Guid id ? RedirectToAction(nameof(Index), new { businessId = id }) : RedirectToAction("Index", "Businesses");
        }
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var now = DateTime.UtcNow.Date;
        var currentMonth = new DateTime(now.Year, now.Month, 1);
        var firstMonth = currentMonth.AddMonths(-5);
        var lastMonthEnd = currentMonth.AddMonths(1);
        // Preserve the C1 accounting rule: reversed entries and reversal markers don't count twice.
        var cash = await Db.CashbookEntries.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.Status == "Posted" &&
                e.EntryType != "Reversal" && e.EntryDate >= firstMonth && e.EntryDate < lastMonthEnd)
            .Select(e => new { e.EntryDate, e.Direction, e.Amount }).ToListAsync(ct);
        var months = Enumerable.Range(0, 6).Select(offset =>
        {
            var start = firstMonth.AddMonths(offset);
            var end = start.AddMonths(1);
            var rows = cash.Where(x => x.EntryDate >= start && x.EntryDate < end).ToList();
            return new MonthlyCashVm
            {
                Month = start.ToString("yyyy-MM"),
                MoneyIn = rows.Where(x => x.Direction == "In").Sum(x => x.Amount),
                MoneyOut = rows.Where(x => x.Direction == "Out").Sum(x => x.Amount)
            };
        }).ToList();
        var invoices = await Db.Invoices.AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.Status != "Draft" && i.Status != "Cancelled")
            .Select(i => new { i.TotalAmount, i.AmountPaid, i.DueDate }).ToListAsync(ct);
        var outstanding = invoices.Sum(i => Math.Max(0m, i.TotalAmount - i.AmountPaid));
        var overdue = invoices.Where(i => i.DueDate.Date < now && i.TotalAmount > i.AmountPaid).ToList();
        var overdueAmount = overdue.Sum(i => i.TotalAmount - i.AmountPaid);
        // Forecast = arithmetic mean of three COMPLETE recent months. Not a predictive ML model.
        var historical = months.Take(5).ToList();
        var lookback = historical.TakeLast(3).ToList();
        decimal? projected = lookback.Count(m => m.MoneyIn != 0m || m.MoneyOut != 0m) == 3
            ? Math.Round(lookback.Average(m => m.Net), 2) : null;
        var vm = new FinanceInsightsVm
        {
            BusinessId = businessId.Value, BusinessName = access.BusinessName,
            OutstandingInvoices = outstanding, OverdueInvoices = overdueAmount,
            OverdueCount = overdue.Count,
            CurrentMonthMoneyIn = months[^1].MoneyIn,
            CurrentMonthMoneyOut = months[^1].MoneyOut,
            EstimatedNextMonthNet = projected, ForecastMonthsUsed = projected.HasValue ? 3 : 0,
            Months = months
        };
        if (vm.OverdueCount > 0) vm.NextActions.Add($"Follow up on {vm.OverdueCount} overdue invoice(s) worth R {vm.OverdueInvoices:N2}.");
        if (vm.CurrentMonthMoneyOut > vm.CurrentMonthMoneyIn)
            vm.NextActions.Add("Review this month's spending: recorded outflows exceed inflows so far.");
        if (projected.HasValue && projected.Value < 0)
            vm.NextActions.Add("Recent monthly net cash movement is negative. Review expenses and expected receipts.");
        if (vm.NextActions.Count == 0) vm.NextActions.Add("Keep recording receipts and expenses to improve the visibility of your business cash flow.");
        Response.Headers.CacheControl = "private, no-store";
        return View(vm);
    }
}
