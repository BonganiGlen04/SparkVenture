using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
public sealed class InvoicesController : FinanceControllerBase
{
    private readonly ILogger<InvoicesController> _logger;
    public InvoicesController(ApplicationDbContext db, UserManager<ApplicationUser> users,
        ILogger<InvoicesController> logger) : base(db, users) => _logger = logger;

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
        var today = DateTime.UtcNow.Date;
        var list = await (from invoice in Db.Invoices.AsNoTracking()
                          join customer in Db.Customers.AsNoTracking() on invoice.CustomerId equals customer.CustomerId
                          where invoice.BusinessId == businessId.Value && customer.BusinessId == businessId.Value
                          orderby invoice.IssueDate descending
                          select new InvoiceRowVm
                          {
                              InvoiceId = invoice.InvoiceId, Number = invoice.InvoiceNumber,
                              CustomerName = customer.DisplayName,
                              Status = invoice.Status != "Draft" && invoice.Status != "Paid" &&
                                  invoice.DueDate < today && invoice.AmountPaid < invoice.TotalAmount ? "Overdue" : invoice.Status,
                              IssueDate = invoice.IssueDate, DueDate = invoice.DueDate,
                              Total = invoice.TotalAmount, Paid = invoice.AmountPaid
                          }).Take(200).ToListAsync(ct);
        return View(new InvoicesIndexVm { BusinessId = businessId.Value,
            BusinessName = access.BusinessName, CanEdit = access.CanWrite, Invoices = list });
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid businessId, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        var model = new InvoiceFormVm { BusinessId = businessId, BusinessName = access.BusinessName };
        await PopulateCustomers(model, ct);
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormVm model, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        model.BusinessName = access.BusinessName;
        await PopulateCustomers(model, ct);
        var total = await ValidateAndCompute(model, ct);
        if (!ModelState.IsValid) return View("Form", model);

        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            // Sequence number allocated once, transactionally. Do not generate numbers in the browser.
            var sequence = await Db.InvoiceNumberSequences
                .SingleOrDefaultAsync(x => x.BusinessId == model.BusinessId, ct);
            if (sequence is null)
            {
                sequence = new InvoiceNumberSequence
                {
                    InvoiceNumberSequenceId = Guid.NewGuid(), BusinessId = model.BusinessId,
                    Prefix = "SV-" + model.BusinessId.ToString("N")[..6] + "-", NextNumber = 1
                };
                Db.InvoiceNumberSequences.Add(sequence);
            }
            var number = sequence.Prefix + sequence.NextNumber.ToString("D4");
            if (number.Length > 20 || sequence.NextNumber >= int.MaxValue)
                return BadRequest("The invoice numbering configuration needs attention.");
            sequence.NextNumber += 1;
            var id = Guid.NewGuid();
            var invoice = new Invoice
            {
                InvoiceId = id, BusinessId = model.BusinessId, CustomerId = model.CustomerId,
                InvoiceNumber = number, Status = "Draft", IssueDate = model.IssueDate.Date,
                DueDate = model.DueDate.Date, Subtotal = total.Subtotal,
                TaxAmount = total.Tax, TotalAmount = total.Total,
                AmountPaid = 0, CurrencyCode = "ZAR"
            };
            Db.Invoices.Add(invoice);
            foreach (var line in model.Lines)
                Db.InvoiceLines.Add(new InvoiceLine
                {
                    InvoiceLineId = Guid.NewGuid(), InvoiceId = id, BusinessId = model.BusinessId,
                    Description = line.Description.Trim(), Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice, LineTotal = Money(line.Quantity * line.UnitPrice)
                });
            FinanceAudit(userId, model.BusinessId, "invoice.created", "Invoice", id);
            await Db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            TempData["SuccessMessage"] = "Draft invoice saved. It has not been emailed or issued.";
            return RedirectToAction(nameof(Details), new { businessId = model.BusinessId, id });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Invoice sequence conflict for business {BusinessId}", model.BusinessId);
            await tx.RollbackAsync(ct);
            ModelState.AddModelError("", "Couldn't reserve an invoice number. Please retry.");
            return View("Form", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        var invoice = await Db.Invoices.AsNoTracking().SingleOrDefaultAsync(
            i => i.BusinessId == businessId && i.InvoiceId == id, ct);
        if (invoice is null || invoice.Status != "Draft") return NotFound();
        var model = new InvoiceFormVm
        {
            BusinessId = businessId, InvoiceId = id, BusinessName = access.BusinessName,
            CustomerId = invoice.CustomerId, IssueDate = invoice.IssueDate, DueDate = invoice.DueDate,
            TaxRatePercent = invoice.Subtotal == 0 ? 0 :
                Math.Round(invoice.TaxAmount / invoice.Subtotal * 100, 2)
        };
        model.Lines = await Db.InvoiceLines.AsNoTracking()
            .Where(l => l.BusinessId == businessId && l.InvoiceId == id)
            .OrderBy(l => l.Description)
            .Select(l => new InvoiceLineVm { Description = l.Description,
                Quantity = l.Quantity, UnitPrice = l.UnitPrice, LineTotal = l.LineTotal }).ToListAsync(ct);
        await PopulateCustomers(model, ct);
        return View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InvoiceFormVm model, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanWrite || model.InvoiceId is null) return NotFound();
        model.BusinessName = access.BusinessName;
        await PopulateCustomers(model, ct);
        var total = await ValidateAndCompute(model, ct);
        if (!ModelState.IsValid) return View("Form", model);
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var invoice = await Db.Invoices.SingleOrDefaultAsync(i => i.BusinessId == model.BusinessId &&
            i.InvoiceId == model.InvoiceId.Value && i.Status == "Draft", ct);
        if (invoice is null) return NotFound();
        if (await Db.PaymentAllocations.AnyAsync(x => x.BusinessId == model.BusinessId && x.InvoiceId == invoice.InvoiceId && x.AllocationStatus == "Posted", ct))
            return BadRequest("A paid invoice cannot be edited.");
        var oldLines = await Db.InvoiceLines.Where(l => l.BusinessId == model.BusinessId && l.InvoiceId == invoice.InvoiceId).ToListAsync(ct);
        Db.InvoiceLines.RemoveRange(oldLines);
        foreach (var l in model.Lines)
            Db.InvoiceLines.Add(new InvoiceLine
            {
                InvoiceLineId = Guid.NewGuid(), InvoiceId = invoice.InvoiceId,
                BusinessId = model.BusinessId, Description = l.Description.Trim(),
                Quantity = l.Quantity, UnitPrice = l.UnitPrice,
                LineTotal = Money(l.Quantity * l.UnitPrice)
            });
        invoice.CustomerId = model.CustomerId;
        invoice.IssueDate = model.IssueDate.Date;
        invoice.DueDate = model.DueDate.Date;
        invoice.Subtotal = total.Subtotal;
        invoice.TaxAmount = total.Tax;
        invoice.TotalAmount = total.Total;
        FinanceAudit(userId, model.BusinessId, "invoice.updated", "Invoice", invoice.InvoiceId);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Draft updated.";
        return RedirectToAction(nameof(Details), new { businessId = model.BusinessId, id = invoice.InvoiceId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var today = DateTime.UtcNow.Date;
        var result = await (from i in Db.Invoices.AsNoTracking()
                            join c in Db.Customers.AsNoTracking() on i.CustomerId equals c.CustomerId
                            where i.BusinessId == businessId && i.InvoiceId == id && c.BusinessId == businessId
                            select new InvoiceDetailsVm
                            {
                                BusinessId = businessId, InvoiceId = i.InvoiceId,
                                BusinessName = access.BusinessName, CustomerName = c.DisplayName,
                                Number = i.InvoiceNumber,
                                Status = i.Status != "Draft" && i.Status != "Paid" &&
                                    i.DueDate < today && i.AmountPaid < i.TotalAmount ? "Overdue" : i.Status,
                                IssueDate = i.IssueDate, DueDate = i.DueDate,
                                Subtotal = i.Subtotal, TaxAmount = i.TaxAmount,
                                Total = i.TotalAmount, Paid = i.AmountPaid,
                                CanEdit = access.CanWrite
                            }).SingleOrDefaultAsync(ct);
        if (result is null) return NotFound();
        result.Lines = await Db.InvoiceLines.AsNoTracking()
            .Where(l => l.BusinessId == businessId && l.InvoiceId == id)
            .OrderBy(l => l.Description)
            .Select(l => new InvoiceLineVm { Description = l.Description,
                Quantity = l.Quantity, UnitPrice = l.UnitPrice, LineTotal = l.LineTotal }).ToListAsync(ct);
        Response.Headers.CacheControl = "private, no-store";
        return View(result);
    }

    // "Issue" updates the internal record only; it does not email a customer or charge them.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var invoice = await Db.Invoices.SingleOrDefaultAsync(i => i.BusinessId == businessId && i.InvoiceId == id, ct);
        if (invoice is null) return NotFound();
        if (invoice.Status != "Draft") return BadRequest("Only a draft may be issued.");
        if (invoice.TotalAmount <= 0 ||
            !await Db.InvoiceLines.AnyAsync(l => l.BusinessId == businessId && l.InvoiceId == id, ct))
            return BadRequest("Invoice needs a positive total and at least one line.");
        invoice.Status = "Sent";
        FinanceAudit(userId, businessId, "invoice.issued", "Invoice", id);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Invoice marked as issued. Share it manually; no email was sent.";
        return RedirectToAction(nameof(Details), new { businessId, id });
    }

    private async Task PopulateCustomers(InvoiceFormVm model, CancellationToken ct) =>
        model.Customers = await Db.Customers.AsNoTracking().Where(c => c.BusinessId == model.BusinessId)
            .OrderBy(c => c.DisplayName)
            .Select(c => new SelectListItem { Text = c.DisplayName, Value = c.CustomerId.ToString() })
            .ToListAsync(ct);

    private static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task<(decimal Subtotal, decimal Tax, decimal Total)> ValidateAndCompute(InvoiceFormVm m, CancellationToken ct)
    {
        if (!await Db.Customers.AsNoTracking().AnyAsync(c => c.CustomerId == m.CustomerId && c.BusinessId == m.BusinessId, ct))
            ModelState.AddModelError(nameof(m.CustomerId), "Choose a customer belonging to this business.");
        if (m.DueDate.Date < m.IssueDate.Date)
            ModelState.AddModelError(nameof(m.DueDate), "Due date must be on/after the issue date.");
        if (m.IssueDate.Date > DateTime.UtcNow.Date.AddDays(365) || m.IssueDate.Date < new DateTime(2000,1,1))
            ModelState.AddModelError(nameof(m.IssueDate), "Choose a valid issue date.");
        if (m.TaxRatePercent != 0m && m.TaxRatePercent != 15m)
            ModelState.AddModelError(nameof(m.TaxRatePercent), "For this MVP select 0% or 15%. Use 15% only if VAT is appropriate for this business.");
        m.Lines?.RemoveAll(l => string.IsNullOrWhiteSpace(l.Description) && l.UnitPrice == 0m);
        if (m.Lines is null || m.Lines.Count is < 1 or > 20)
        {
            ModelState.AddModelError(nameof(m.Lines), "Enter 1–20 invoice lines.");
            return (0, 0, 0);
        }
        decimal subtotal = 0;
        for (var n = 0; n < m.Lines.Count; n++)
        {
            var line = m.Lines[n];
            if (string.IsNullOrWhiteSpace(line.Description)) ModelState.AddModelError($"Lines[{n}].Description", "Description required.");
            if (line.Quantity <= 0 || line.Quantity > 99999999) ModelState.AddModelError($"Lines[{n}].Quantity", "Enter a valid positive quantity.");
            if (line.UnitPrice < 0 || line.UnitPrice > 99999999) ModelState.AddModelError($"Lines[{n}].UnitPrice", "Enter a valid price.");
            if (line.Quantity > 0 && line.UnitPrice >= 0) subtotal += Money(line.Quantity * line.UnitPrice);
        }
        subtotal = Money(subtotal);
        if (subtotal <= 0 || subtotal > 999999999) ModelState.AddModelError(nameof(m.Lines), "Invoice total must be positive and within allowed range.");
        var tax = Money(subtotal * m.TaxRatePercent / 100);
        return (subtotal, tax, subtotal + tax);
    }
}
