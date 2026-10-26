using System;
using System.Linq;
using System.Text.Json;
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
public sealed class CustomersController : FinanceControllerBase
{
    private static readonly string[] Types = { "Individual", "Business", "Government", "Institution" };
    public CustomersController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, string? search, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        if (businessId is null)
        {
            var selected = await DefaultBusinessAsync(userId, ct);
            return selected is Guid id ? RedirectToAction(nameof(Index), new { businessId = id }) : RedirectToAction("Index", "Businesses");
        }
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var q = Db.Customers.AsNoTracking().Where(c => c.BusinessId == businessId.Value);
        search = search?.Trim();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(c => c.DisplayName.Contains(search));
        var items = await q.OrderBy(c => c.DisplayName).Take(200)
            .Select(c => new CustomerListItemVm
            {
                CustomerId = c.CustomerId, Name = c.DisplayName,
                CustomerType = c.CustomerType, Email = c.Email, Phone = c.Phone
            }).ToListAsync(ct);
        return View(new CustomersIndexVm
        {
            BusinessId = businessId.Value, BusinessName = access.BusinessName,
            CanEdit = access.CanWrite, Search = search ?? "", Customers = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid businessId, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        return View("Form", new CustomerFormVm { BusinessId = businessId, BusinessName = access.BusinessName });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormVm model, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        model.BusinessName = access.BusinessName;
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);
        var id = Guid.NewGuid();
        Db.Customers.Add(new Customer
        {
            CustomerId = id, BusinessId = model.BusinessId, DisplayName = model.DisplayName.Trim(),
            CustomerType = model.CustomerType, Email = Normalize(model.Email), Phone = Normalize(model.Phone),
            AvgDaysToPay = 0, LateInvoiceRate = 0
        });
        FinanceAudit(userId, model.BusinessId, "customer.created", "Customer", id);
        await Db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Customer saved.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanWrite) return NotFound();
        var item = await Db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.BusinessId == businessId && c.CustomerId == id, ct);
        if (item is null) return NotFound();
        return View("Form", new CustomerFormVm
        {
            BusinessId = businessId, CustomerId = item.CustomerId, BusinessName = access.BusinessName,
            DisplayName = item.DisplayName, CustomerType = item.CustomerType,
            Email = item.Email, Phone = item.Phone
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerFormVm model, CancellationToken ct)
    {
        if (!CurrentUserId(out var userId)) return Challenge();
        var access = await AccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanWrite || model.CustomerId is null) return NotFound();
        model.BusinessName = access.BusinessName;
        Validate(model);
        if (!ModelState.IsValid) return View("Form", model);
        var customer = await Db.Customers.FirstOrDefaultAsync(
            c => c.BusinessId == model.BusinessId && c.CustomerId == model.CustomerId.Value, ct);
        if (customer is null) return NotFound();
        customer.DisplayName = model.DisplayName.Trim();
        customer.CustomerType = model.CustomerType;
        customer.Email = Normalize(model.Email);
        customer.Phone = Normalize(model.Phone);
        FinanceAudit(userId, model.BusinessId, "customer.updated", "Customer", customer.CustomerId);
        await Db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Customer updated.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }

    private void Validate(CustomerFormVm m)
    {
        if (!Types.Contains(m.CustomerType)) ModelState.AddModelError(nameof(m.CustomerType), "Choose a valid customer type.");
        if (string.IsNullOrWhiteSpace(m.DisplayName)) ModelState.AddModelError(nameof(m.DisplayName), "Enter a customer name.");
        if (m.CustomerId == Guid.Empty) ModelState.AddModelError(nameof(m.CustomerId), "Invalid customer.");
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
