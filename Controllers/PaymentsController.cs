using System;
using System.Data;
using System.Linq;
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
public sealed class PaymentsController : FinanceControllerBase
{
    private readonly ILogger<PaymentsController> _logger;
    public PaymentsController(ApplicationDbContext db, UserManager<ApplicationUser> users,
        ILogger<PaymentsController> logger) : base(db, users) => _logger = logger;

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
        var rows = await (
            from payment in Db.PaymentTransactions.AsNoTracking()
            join allocation in Db.PaymentAllocations.AsNoTracking()
                on payment.PaymentTransactionId equals allocation.PaymentTransactionId
            join invoice in Db.Invoices.AsNoTracking() on allocation.InvoiceId equals invoice.InvoiceId
            where payment.BusinessId == businessId.Value && allocation.BusinessId == businessId.Value &&
                  invoice.BusinessId == businessId.Value
            orderby payment.ReceivedOn descending
            select new PaymentRowVm
            {
                PaymentId = payment.PaymentTransactionId, InvoiceNumber = invoice.InvoiceNumber,
                ReceivedOn = payment.ReceivedOn, Amount = allocation.AllocatedAmount,
                Reference = payment.Reference, Method = payment.PaymentMethod, Status = payment.Status,
                CanReverse = payment.ProviderCode == "MANUAL" && payment.Status == "Completed" && allocation.AllocationStatus == "Posted"
            }).Take(200).ToListAsync(ct);
        foreach (var r in rows) r.CanReverse &= access.CanWrite;
        return View(new PaymentsIndexVm
        {
            BusinessId = businessId.Value, BusinessName = access.BusinessName,
            CanEdit = access.CanWrite, Payments = rows
        });
    }

    [HttpGet]
    public async Task<IActionResult> Record(Guid businessId, Guid? invoiceId, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        var model = new PaymentRecordVm
        {
            BusinessId = businessId, BusinessName = access.BusinessName, InvoiceId = invoiceId ?? Guid.Empty
        };
        await PopulateInvoices(model, ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(PaymentRecordVm model, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        model.BusinessName = access.BusinessName;
        await PopulateInvoices(model, ct);
        if (model.PaymentMethod is not ("EFT" or "Cash" or "Card" or "Mobile"))
            ModelState.AddModelError(nameof(model.PaymentMethod), "Select a supported payment method.");
        if (!model.ConfirmReceived)
            ModelState.AddModelError(nameof(model.ConfirmReceived), "Only record funds you have actually received.");
        if (model.ReceivedOn.Date > DateTime.UtcNow.Date || model.ReceivedOn.Date < DateTime.UtcNow.Date.AddYears(-5))
            ModelState.AddModelError(nameof(model.ReceivedOn), "Enter a valid date on or before today.");
        if (string.IsNullOrWhiteSpace(model.Reference) || model.Reference.Trim().Length > 50)
            ModelState.AddModelError(nameof(model.Reference), "Enter a unique reference of up to 50 characters.");
        if (model.Amount != Math.Round(model.Amount, 2, MidpointRounding.AwayFromZero))
            ModelState.AddModelError(nameof(model.Amount), "Enter an amount to two decimal places.");
        if (!ModelState.IsValid) return View(model);

        // Serializable isolation ensures two concurrent receipts cannot allocate the same outstanding amount.
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var invoice = await Db.Invoices.SingleOrDefaultAsync(i => i.BusinessId == model.BusinessId &&
            i.InvoiceId == model.InvoiceId, ct);
        if (invoice is null || invoice.Status is "Draft" or "Cancelled" or "Rejected")
        {
            ModelState.AddModelError(nameof(model.InvoiceId), "Choose an issued invoice.");
            return View(model);
        }
        var reference = model.Reference.Trim();
        if (await Db.PaymentTransactions.AnyAsync(p => p.BusinessId == model.BusinessId &&
            p.ProviderCode == "MANUAL" && p.Reference == reference, ct))
        {
            ModelState.AddModelError(nameof(model.Reference), "This reference already exists for the business.");
            return View(model);
        }
        var posted = await Db.PaymentAllocations.Where(a => a.BusinessId == model.BusinessId &&
            a.InvoiceId == invoice.InvoiceId && a.AllocationStatus == "Posted")
            .SumAsync(a => (decimal?)a.AllocatedAmount, ct) ?? 0m;
        var outstanding = invoice.TotalAmount - posted;
        if (model.Amount <= 0m || model.Amount > outstanding || outstanding <= 0)
        {
            ModelState.AddModelError(nameof(model.Amount), $"Amount must not exceed the outstanding balance of R {Math.Max(0m, outstanding):N2}.");
            return View(model);
        }
        var now = DateTime.UtcNow;
        var paymentId = Guid.NewGuid();
        var allocationId = Guid.NewGuid();
        var receivedDate = model.ReceivedOn.Date;
        Db.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentTransactionId = paymentId, BusinessId = model.BusinessId,
            InitiatedByUserId = userId, ProviderCode = "MANUAL",
            PaymentMethod = model.PaymentMethod, ReceivedOn = receivedDate,
            Reference = reference, Amount = model.Amount,
            CurrencyCode = "ZAR", Status = "Completed", CreatedAtUtc = now
        });
        Db.PaymentAllocations.Add(new PaymentAllocation
        {
            PaymentAllocationId = allocationId, PaymentTransactionId = paymentId,
            InvoiceId = invoice.InvoiceId, BusinessId = model.BusinessId,
            AllocatedAmount = model.Amount, AllocationStatus = "Posted",
            CreatedAtUtc = now, PostedAtUtc = now
        });
        // One invoice allocation => exactly one cashbook receipt.
        Db.CashbookEntries.Add(new CashbookEntry
        {
            CashbookEntryId = Guid.NewGuid(), BusinessId = model.BusinessId,
            CustomerId = invoice.CustomerId, InvoiceId = invoice.InvoiceId,
            PaymentAllocationId = allocationId, CreatedByUserId = userId,
            Direction = "In", EntryType = "InvoicePayment", Status = "Posted",
            Amount = model.Amount, EntryDate = receivedDate,
            Description = "Payment received for " + invoice.InvoiceNumber,
            Reference = reference, CreatedAtUtc = now
        });
        invoice.AmountPaid = posted + model.Amount;
        if (invoice.AmountPaid >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
            invoice.FullyPaidAtUtc = now;
        }
        else
        {
            invoice.Status = "PartiallyPaid";
            invoice.FullyPaidAtUtc = null;
        }
        FinanceAudit(userId, model.BusinessId, "payment.recorded_and_allocated", "PaymentTransaction", paymentId);
        try
        {
            await Db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Payment posting conflict for {BusinessId}", model.BusinessId);
            await tx.RollbackAsync(ct);
            ModelState.AddModelError("", "Payment could not be posted. Verify it was not recorded already, then retry.");
            return View(model);
        }
        TempData["SuccessMessage"] = "Receipt recorded, allocated to the invoice and posted to Cashbook once.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }

    // Undo only manual receipts created by SparkVenture. Never reverse external gateway records here.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var payment = await Db.PaymentTransactions.SingleOrDefaultAsync(p => p.BusinessId == businessId &&
            p.PaymentTransactionId == id && p.ProviderCode == "MANUAL" && p.Status == "Completed", ct);
        if (payment is null) return NotFound();
        var allocations = await Db.PaymentAllocations.Where(a => a.BusinessId == businessId &&
            a.PaymentTransactionId == payment.PaymentTransactionId && a.AllocationStatus == "Posted")
            .ToListAsync(ct);
        if (allocations.Count != 1) return BadRequest("This payment cannot be reversed through the simple manual workflow.");
        var allocation = allocations[0];
        var invoice = await Db.Invoices.SingleOrDefaultAsync(i => i.BusinessId == businessId &&
            i.InvoiceId == allocation.InvoiceId, ct);
        if (invoice is null) return NotFound();
        var original = await Db.CashbookEntries.SingleOrDefaultAsync(e => e.BusinessId == businessId &&
            e.PaymentAllocationId == allocation.PaymentAllocationId && e.Status == "Posted" && e.EntryType == "InvoicePayment", ct);
        if (original is null || await Db.CashbookEntries.AnyAsync(e => e.BusinessId == businessId &&
            e.ReversesCashbookEntryId == original.CashbookEntryId, ct))
            return BadRequest("The receipt is missing, already reversed, or requires manual investigation.");
        payment.Status = "Reversed";
        allocation.AllocationStatus = "Reversed";
        original.Status = "Reversed";
        var now = DateTime.UtcNow;
        Db.CashbookEntries.Add(new CashbookEntry
        {
            CashbookEntryId = Guid.NewGuid(), BusinessId = businessId,
            CreatedByUserId = userId, Direction = "Out", EntryType = "Reversal", Status = "Posted",
            Amount = original.Amount, EntryDate = now.Date,
            Description = "Reversal of " + invoice.InvoiceNumber,
            Reference = payment.Reference, ReversesCashbookEntryId = original.CashbookEntryId,
            CreatedAtUtc = now
        });
        var remainingPaid = await Db.PaymentAllocations.Where(a => a.BusinessId == businessId &&
            a.InvoiceId == invoice.InvoiceId && a.PaymentAllocationId != allocation.PaymentAllocationId &&
            a.AllocationStatus == "Posted")
            .SumAsync(a => (decimal?)a.AllocatedAmount, ct) ?? 0m;
        invoice.AmountPaid = remainingPaid;
        invoice.FullyPaidAtUtc = null;
        invoice.Status = remainingPaid >= invoice.TotalAmount ? "Paid" : remainingPaid > 0 ? "PartiallyPaid" :
            invoice.DueDate.Date < now.Date ? "Overdue" : "Sent";
        FinanceAudit(userId, businessId, "payment.reversed", "PaymentTransaction", id);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Manual payment reversed; invoice and cashbook updated.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    private async Task PopulateInvoices(PaymentRecordVm model, CancellationToken ct)
    {
        var available = await Db.Invoices.AsNoTracking()
            .Where(i => i.BusinessId == model.BusinessId && i.Status != "Draft" && i.Status != "Cancelled" &&
                        i.TotalAmount > i.AmountPaid)
            .OrderByDescending(i => i.IssueDate).Take(150)
            .Select(i => new { i.InvoiceId, i.InvoiceNumber, i.TotalAmount, i.AmountPaid })
            .ToListAsync(ct);
        model.Invoices = available.Select(i => new SelectListItem
        {
            Value = i.InvoiceId.ToString(),
            Text = $"{i.InvoiceNumber} — R {i.TotalAmount - i.AmountPaid:N2} outstanding"
        }).ToList();
    }
}
