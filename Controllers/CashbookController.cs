using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class CashbookController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<CashbookController> _logger;
    private static readonly string[] EditableRoles = { "Owner", "Manager", "Accountant" };

    public CashbookController(ApplicationDbContext db,
        UserManager<ApplicationUser> users, ILogger<CashbookController> logger)
    {
        _db = db;
        _users = users;
        _logger = logger;
    }

    private sealed class BusinessAccess
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool CanEdit => EditableRoles.Contains(Role);
    }

    private bool GetUserId(out Guid userId) =>
        Guid.TryParse(_users.GetUserId(User), out userId);

    private Task<BusinessAccess?> AccessAsync(Guid businessId, Guid userId, CancellationToken ct) =>
        (from member in _db.BusinessMembers.AsNoTracking()
         join business in _db.Businesses.AsNoTracking() on member.BusinessId equals business.BusinessId
         where member.UserId == userId && member.BusinessId == businessId &&
               member.MembershipStatus == "Active"
         select new BusinessAccess { Name = business.TradingName ?? "Business", Role = member.BusinessRole })
        .FirstOrDefaultAsync(ct);

    private async Task PopulateCategoriesAsync(CashbookCreateViewModel model, CancellationToken ct)
    {
        model.Categories = await _db.ExpenseCategories.AsNoTracking()
            .Where(c => (c.BusinessId == null || c.BusinessId == model.BusinessId) && c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.ExpenseCategoryId.ToString(), Text = c.Name })
            .ToListAsync(ct);
    }

    // Only normal Posted entries belong to accounting totals. Reversal rows are
    // historical adjustment markers; the original is marked Reversed atomically.
    // This also handles the seed's duplicate-rent reversal without double counting.
    private IQueryable<CashbookEntry> EffectiveEntries(Guid businessId) =>
        _db.CashbookEntries.AsNoTracking().Where(e =>
            e.BusinessId == businessId && e.Status == "Posted" && e.EntryType != "Reversal");

    private static CashbookEntryRowViewModel ToRow(CashbookEntry e, string? category = null) => new()
    {
        CashbookEntryId = e.CashbookEntryId,
        EntryDate = e.EntryDate,
        Direction = e.Direction ?? "",
        EntryType = e.EntryType ?? "",
        Status = e.Status ?? "",
        Amount = e.Amount,
        Description = e.Description ?? "",
        Reference = e.Reference,
        CategoryName = category,
        ReversesCashbookEntryId = e.ReversesCashbookEntryId,
        CanReverse = e.Status == "Posted" && e.PaymentAllocationId == null &&
                     e.InvoiceId == null && (e.EntryType == "Expense" || e.EntryType == "ManualIncome")
    };

    // GET /Cashbook/Index?businessId=...&month=2026-10
    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, string? month, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        if (businessId is null)
        {
            var ids = await _db.BusinessMembers.AsNoTracking()
                .Where(x => x.UserId == userId && x.MembershipStatus == "Active")
                .Select(x => x.BusinessId).Distinct().Take(2).ToListAsync(ct);
            if (ids.Count != 1) return RedirectToAction("Index", "Businesses");
            return RedirectToAction(nameof(Index), new { businessId = ids[0] });
        }
        if (businessId == Guid.Empty) return NotFound();
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();

        var start = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed) ? new DateTime(parsed.Year, parsed.Month, 1)
            : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        if (start.Year < 2000 || start.Year > 2100)
            return BadRequest("Select a month between 2000 and 2100.");
        var end = start.AddMonths(1);
        var q = EffectiveEntries(businessId.Value)
            .Where(e => e.EntryDate >= start && e.EntryDate < end);

        var totals = await q.GroupBy(e => e.Direction).Select(g => new
        {
            Direction = g.Key, Total = g.Sum(e => e.Amount), Count = g.Count()
        }).ToListAsync(ct);
        var entries = await _db.CashbookEntries.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.EntryDate >= start && e.EntryDate < end)
            .OrderByDescending(e => e.EntryDate).ThenByDescending(e => e.CreatedAtUtc)
            .Take(150).ToListAsync(ct);
        var categories = await _db.ExpenseCategories.AsNoTracking()
            .Where(c => c.BusinessId == null || c.BusinessId == businessId)
            .ToDictionaryAsync(c => c.ExpenseCategoryId, c => c.Name, ct);
        var rows = entries.Select(e =>
        {
            var row = ToRow(e, e.ExpenseCategoryId is Guid id && categories.TryGetValue(id, out var cat)
                ? cat : null);
            row.CanReverse &= access.CanEdit;
            return row;
        }).ToList();
        return View(new CashbookIndexViewModel
        {
            BusinessId = businessId.Value,
            BusinessName = access.Name,
            Role = access.Role,
            CanEdit = access.CanEdit,
            Month = start.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            MoneyIn = totals.Where(x => x.Direction == "In").Sum(x => x.Total),
            MoneyOut = totals.Where(x => x.Direction == "Out").Sum(x => x.Total),
            TotalEntries = totals.Sum(x => x.Count),
            Entries = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid businessId, string direction = "Out", CancellationToken ct = default)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanEdit) return NotFound();
        var model = new CashbookCreateViewModel
        {
            BusinessId = businessId, BusinessName = access.Name,
            Direction = direction == "In" ? "In" : "Out"
        };
        await PopulateCategoriesAsync(model, ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CashbookCreateViewModel model, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanEdit) return NotFound();
        model.BusinessName = access.Name;
        await PopulateCategoriesAsync(model, ct);

        if (model.Direction != "In" && model.Direction != "Out")
            ModelState.AddModelError(nameof(model.Direction), "Choose income or expense.");
        if (model.EntryDate.Date > DateTime.UtcNow.Date.AddDays(1) ||
            model.EntryDate.Date < new DateTime(2000, 1, 1))
            ModelState.AddModelError(nameof(model.EntryDate), "Select a valid transaction date.");
        if (model.Amount > 0 && decimal.Round(model.Amount, 2) != model.Amount)
            ModelState.AddModelError(nameof(model.Amount), "Use no more than two decimal places.");

        if (model.Direction == "Out")
        {
            if (!model.ExpenseCategoryId.HasValue || model.ExpenseCategoryId == Guid.Empty)
                ModelState.AddModelError(nameof(model.ExpenseCategoryId), "Choose an expense category.");
            else
            {
                var validCategory = await _db.ExpenseCategories.AsNoTracking().AnyAsync(c =>
                    c.ExpenseCategoryId == model.ExpenseCategoryId && c.IsActive &&
                    (c.BusinessId == null || c.BusinessId == model.BusinessId), ct);
                if (!validCategory) ModelState.AddModelError(nameof(model.ExpenseCategoryId), "Invalid category.");
            }
        }
        if (!ModelState.IsValid) return View(model);

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        _db.CashbookEntries.Add(new CashbookEntry
        {
            CashbookEntryId = id,
            BusinessId = model.BusinessId,
            CreatedByUserId = userId,
            Direction = model.Direction,
            EntryType = model.Direction == "In" ? "ManualIncome" : "Expense",
            Status = "Posted",
            Amount = model.Amount,
            EntryDate = model.EntryDate.Date,
            Description = model.Description.Trim(),
            Reference = string.IsNullOrWhiteSpace(model.Reference) ? null : model.Reference.Trim(),
            ExpenseCategoryId = model.Direction == "Out" ? model.ExpenseCategoryId : null,
            CreatedAtUtc = now
        });
        _db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = model.BusinessId,
            OccurredAtUtc = now, ActionCode = "cashbook.entry_created", EntityType = "CashbookEntry",
            EntityId = id.ToString(), PayloadJson = JsonSerializer.Serialize(new { direction = model.Direction, amount = model.Amount })
        });
        try { await _db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Cashbook save failure for {BusinessId}", model.BusinessId);
            ModelState.AddModelError("", "Could not save this entry. Please try again.");
            return View(model);
        }
        TempData["SuccessMessage"] = "Cashbook entry saved.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId,
            month = model.EntryDate.ToString("yyyy-MM", CultureInfo.InvariantCulture) });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var entry = await _db.CashbookEntries.AsNoTracking()
            .SingleOrDefaultAsync(e => e.BusinessId == businessId && e.CashbookEntryId == id, ct);
        if (entry is null) return NotFound();
        string? categoryName = null;
        if (entry.ExpenseCategoryId is Guid categoryId)
            categoryName = await _db.ExpenseCategories.AsNoTracking()
                .Where(c => c.ExpenseCategoryId == categoryId &&
                    (c.BusinessId == null || c.BusinessId == businessId))
                .Select(c => c.Name).FirstOrDefaultAsync(ct);
        var row = ToRow(entry, categoryName);
        row.CanReverse &= access.CanEdit;
        return View(new CashbookEntryDetailsViewModel
        {
            BusinessId = businessId, BusinessName = access.Name,
            CanEdit = access.CanEdit, Entry = row
        });
    }

    // Corrections never erase financial history or touch invoice-driven payments.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanEdit) return NotFound();

        await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        var original = await _db.CashbookEntries.SingleOrDefaultAsync(e =>
            e.BusinessId == businessId && e.CashbookEntryId == id, ct);
        if (original is null) return NotFound();
        if (original.Status != "Posted" || original.PaymentAllocationId != null ||
            original.InvoiceId != null || (original.EntryType != "Expense" && original.EntryType != "ManualIncome") ||
            await _db.CashbookEntries.AnyAsync(e => e.ReversesCashbookEntryId == id, ct))
            return BadRequest("Only a posted, manual and not-yet-reversed entry can be reversed.");

        var reversedId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        original.Status = "Reversed";
        _db.CashbookEntries.Add(new CashbookEntry
        {
            CashbookEntryId = reversedId, BusinessId = businessId,
            CreatedByUserId = userId,
            Direction = original.Direction == "In" ? "Out" : "In",
            EntryType = "Reversal", Status = "Posted", Amount = original.Amount,
            EntryDate = now.Date,
            Description = ("Reversal: " + (original.Description ?? "Cashbook entry"))[..Math.Min(200, ("Reversal: " + (original.Description ?? "Cashbook entry")).Length)],
            Reference = original.Reference,
            ExpenseCategoryId = original.ExpenseCategoryId,
            ReversesCashbookEntryId = original.CashbookEntryId,
            CreatedAtUtc = now
        });
        _db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            OccurredAtUtc = now, ActionCode = "cashbook.entry_reversed", EntityType = "CashbookEntry",
            EntityId = id.ToString(), PayloadJson = JsonSerializer.Serialize(new { reversalId = reversedId })
        });
        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Entry reversed. Its history has been retained. Add a new entry if you need to correct it.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    [HttpGet]
    public async Task<IActionResult> Categories(Guid businessId, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var categories = await _db.ExpenseCategories.AsNoTracking()
            .Where(c => (c.BusinessId == null || c.BusinessId == businessId) && c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new CashbookCategoryViewModel
            {
                ExpenseCategoryId = c.ExpenseCategoryId,
                Name = c.Name, Code = c.Code, IsGlobal = c.BusinessId == null
            }).ToListAsync(ct);
        return View(new CashbookCategoriesViewModel
        {
            BusinessId = businessId, BusinessName = access.Name,
            CanEdit = access.CanEdit, Categories = categories
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CashbookCategoriesViewModel model, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanEdit) return NotFound();
        var code = (model.NewCategoryCode ?? "").Trim().ToUpperInvariant();
        var name = (model.NewCategoryName ?? "").Trim();
        if (name.Length is < 2 or > 100 || code.Length is < 2 or > 30 ||
            !code.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
        {
            TempData["ErrorMessage"] = "Enter a category name (2–100 characters) and code (2–30 letters/numbers).";
            return RedirectToAction(nameof(Categories), new { businessId = model.BusinessId });
        }
        var exists = await _db.ExpenseCategories.AnyAsync(c => c.Code == code &&
            (c.BusinessId == null || c.BusinessId == model.BusinessId), ct);
        if (exists)
        {
            TempData["ErrorMessage"] = "That category code already exists for this business.";
            return RedirectToAction(nameof(Categories), new { businessId = model.BusinessId });
        }
        _db.ExpenseCategories.Add(new ExpenseCategory
        {
            ExpenseCategoryId = Guid.NewGuid(), BusinessId = model.BusinessId,
            Code = code, Name = name, IsActive = true, SortOrder = 100
        });
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Expense category created.";
        return RedirectToAction(nameof(Categories), new { businessId = model.BusinessId });
    }

    [HttpGet]
    public async Task<IActionResult> Reports(Guid businessId, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var current = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var from = current.AddMonths(-5);
        var records = await EffectiveEntries(businessId)
            .Where(e => e.EntryDate >= from && e.EntryDate < current.AddMonths(1))
            .Select(e => new { e.EntryDate, e.Direction, e.Amount, e.ExpenseCategoryId })
            .ToListAsync(ct);
        var months = Enumerable.Range(0, 6).Select(i =>
        {
            var date = from.AddMonths(i);
            var entries = records.Where(e => e.EntryDate.Year == date.Year && e.EntryDate.Month == date.Month).ToList();
            return new CashbookMonthSummaryViewModel
            {
                Month = date.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Income = entries.Where(e => e.Direction == "In").Sum(e => e.Amount),
                Expenses = entries.Where(e => e.Direction == "Out").Sum(e => e.Amount)
            };
        }).ToList();
        var max = Math.Max(1m, months.Max(m => Math.Max(m.Income, m.Expenses)));
        foreach (var month in months)
        {
            month.IncomeWidth = (int)Math.Round(month.Income * 100m / max);
            month.ExpensesWidth = (int)Math.Round(month.Expenses * 100m / max);
        }
        var categoryNames = await _db.ExpenseCategories.AsNoTracking()
            .Where(c => c.BusinessId == null || c.BusinessId == businessId)
            .ToDictionaryAsync(c => c.ExpenseCategoryId, c => c.Name, ct);
        var categories = records.Where(e => e.Direction == "Out")
            .GroupBy(e => e.ExpenseCategoryId)
            .Select(g => new CashbookCategoryTotalViewModel
            {
                Category = g.Key is Guid key && categoryNames.TryGetValue(key, out var name) ? name : "Other",
                Amount = g.Sum(e => e.Amount)
            }).OrderByDescending(e => e.Amount).ToList();
        return View(new CashbookReportViewModel
        {
            BusinessId = businessId, BusinessName = access.Name,
            Months = months, ExpenseCategories = categories,
            SixMonthIncome = months.Sum(m => m.Income),
            SixMonthExpenses = months.Sum(m => m.Expenses)
        });
    }

    // Download a filtered, authorised cashbook export; Excel formula injection mitigated.
    [HttpGet]
    public async Task<IActionResult> Export(Guid businessId, string? month, CancellationToken ct)
    {
        if (!GetUserId(out var userId)) return Challenge();
        if (await AccessAsync(businessId, userId, ct) is null) return NotFound();
        var start = DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed) ? new DateTime(parsed.Year, parsed.Month, 1)
            : new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        if (start.Year < 2000 || start.Year > 2100)
            return BadRequest("Select a month between 2000 and 2100.");
        var entries = await _db.CashbookEntries.AsNoTracking()
            .Where(e => e.BusinessId == businessId && e.EntryDate >= start && e.EntryDate < start.AddMonths(1))
            .OrderBy(e => e.EntryDate).ThenBy(e => e.CreatedAtUtc)
            .Take(5000).ToListAsync(ct);
        static string Cell(string? text)
        {
            var value = text ?? "";
            if (value.Length > 0 && "=+-@\t\r".Contains(value[0])) value = "'" + value;
            return "\"" + value.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " ") + "\"";
        }
        var csv = new StringBuilder("Date,Direction,Type,Status,Amount,Description,Reference\r\n");
        foreach (var e in entries)
            csv.Append(string.Join(",", Cell(e.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Cell(e.Direction), Cell(e.EntryType), Cell(e.Status),
                e.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                Cell(e.Description), Cell(e.Reference))).Append("\r\n");
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(new UTF8Encoding(true).GetBytes(csv.ToString()), "text/csv",
            $"sparkventure-cashbook-{start:yyyy-MM}.csv");
    }
}
