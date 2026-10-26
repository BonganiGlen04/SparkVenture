using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

// This is the SAME controller as B1, extended through a partial class.
// The accompanying B1 controller is changed only from 'sealed class' to 'sealed partial class'.
public sealed partial class RegistrationApplicationsController
{
    private static readonly string[] EditableWizardKeys =
        { "proposed-names", "directors", "shareholders", "addresses", "year-end", "review" };

    private static string? NextKey(string key) => key switch
    {
        "proposed-names" => "directors",
        "directors" => "shareholders",
        "shareholders" => "addresses",
        "addresses" => "year-end",
        "year-end" => "review",
        _ => null
    };

    private async Task<RegistrationApplication?> AccessibleApplicationAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty || !TryGetUserId(out var userId)) return null;
        var app = await _db.RegistrationApplications.SingleOrDefaultAsync(a =>
            a.RegistrationApplicationId == id && a.WorkflowType == CompanyWorkflow, ct);
        return app is not null && await CanManageAsync(app.BusinessId, userId, ct) ? app : null;
    }

    private static bool CanEditRegistration(RegistrationApplication app) =>
        app.Status is "Draft" or "Returned";

    private async Task<T> DecorateAsync<T>(T model, RegistrationApplication app,
        string key, int stepNumber, string title, CancellationToken ct)
        where T : RegistrationWizardPageViewModel
    {
        model.ApplicationId = app.RegistrationApplicationId;
        model.StepKey = key;
        model.StepNumber = stepNumber;
        model.StepTitle = title;
        model.BusinessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == app.BusinessId)
            .Select(b => b.TradingName).SingleAsync(ct);
        return model;
    }

    // GET /RegistrationApplications/Resume?id=... — returns to the saved application.
    [HttpGet]
    public async Task<IActionResult> Resume(Guid id, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(id, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id });
        var key = app.CurrentStepKey;
        if (key is "business-details" or "") key = "proposed-names";
        if (key is "documents" or "review" or "submit")
        {
            var hasYearEnd = await _db.RegistrationFinancialYearEnds.AsNoTracking()
                .AnyAsync(x => x.RegistrationApplicationId == id, cancellationToken);
            return hasYearEnd
                ? RedirectToAction(nameof(Summary), new { id })
                : RedirectToAction(nameof(Step), new { id, key = "year-end" });
        }
        return EditableWizardKeys.Contains(key)
            ? key == "review"
                ? RedirectToAction(nameof(Summary), new { id })
                : RedirectToAction(nameof(Step), new { id, key })
            : RedirectToAction(nameof(Step), new { id, key = "proposed-names" });
    }

    // GET /RegistrationApplications/Step?id=...&key=directors
    [HttpGet]
    public async Task<IActionResult> Step(Guid id, string key = "proposed-names",
        CancellationToken cancellationToken = default)
    {
        var app = await AccessibleApplicationAsync(id, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id });

        switch (key)
        {
            case "year-end":
            {
                var month = await _db.RegistrationFinancialYearEnds.AsNoTracking()
                    .Where(x => x.RegistrationApplicationId == id)
                    .Select(x => (int?)x.YearEndMonth).SingleOrDefaultAsync(cancellationToken);
                var model = await DecorateAsync(new RegistrationYearEndInput
                {
                    YearEndMonth = month
                }, app, key, 5, "Financial year-end", cancellationToken);
                return View("YearEnd", model);
            }
            case "proposed-names":
            {
                var names = await _db.RegistrationProposedNames.AsNoTracking()
                    .Where(x => x.RegistrationApplicationId == id)
                    .OrderBy(x => x.PriorityOrder)
                    .Select(x => new { x.ProposedName, x.PriorityOrder })
                    .ToListAsync(cancellationToken);
                var business = await _db.Businesses.AsNoTracking()
                    .Where(b => b.BusinessId == app.BusinessId)
                    .Select(b => new { b.TradingName, b.LegalName })
                    .SingleAsync(cancellationToken);
                var model = await DecorateAsync(new RegistrationNamesInput
                {
                    Name1 = names.FirstOrDefault(x => x.PriorityOrder == 1)?.ProposedName,
                    Name2 = names.FirstOrDefault(x => x.PriorityOrder == 2)?.ProposedName,
                    Name3 = names.FirstOrDefault(x => x.PriorityOrder == 3)?.ProposedName,
                    TradingName = business.TradingName,
                    LegalName = business.LegalName,
                    ConfirmBusinessDetails = await _db.RegistrationApplicationTasks.AsNoTracking()
                        .AnyAsync(t => t.RegistrationApplicationId == id &&
                            t.TaskKey == "business-details" && t.Status == "Completed", cancellationToken)
                }, app, key, 1, "Proposed company names", cancellationToken);
                return View("ProposedNames", model);
            }
            case "directors":
            {
                var directors = await _db.RegistrationDirectors.AsNoTracking()
                    .Where(x => x.RegistrationApplicationId == id)
                    .OrderBy(x => x.FullName)
                    .Select(x => new RegistrationDirectorInput
                    {
                        Id = x.RegistrationDirectorId, FullName = x.FullName,
                        Email = x.Email, ContactNumber = x.ContactNumber
                    }).ToListAsync(cancellationToken);
                if (directors.Count == 0) directors.Add(new RegistrationDirectorInput());
                var model = await DecorateAsync(new RegistrationDirectorsInput { Directors = directors },
                    app, key, 2, "Company directors", cancellationToken);
                return View("Directors", model);
            }
            case "shareholders":
            {
                var shareholders = await _db.RegistrationShareholders.AsNoTracking()
                    .Where(x => x.RegistrationApplicationId == id)
                    .OrderBy(x => x.DisplayName)
                    .Select(x => new RegistrationShareholderInput
                    {
                        Id = x.RegistrationShareholderId,
                        ShareholderType = x.ShareholderType,
                        DisplayName = x.DisplayName,
                        SharePercentage = x.SharePercentage
                    }).ToListAsync(cancellationToken);
                if (shareholders.Count == 0) shareholders.Add(new RegistrationShareholderInput());
                var model = await DecorateAsync(new RegistrationShareholdersInput
                    { Shareholders = shareholders }, app, key, 3, "Shareholding", cancellationToken);
                return View("Shareholders", model);
            }
            case "addresses":
            {
                var address = await _db.RegistrationApplicationAddresses.AsNoTracking()
                    .Where(x => x.RegistrationApplicationId == id && x.AddressType == "RegisteredOffice")
                    .Select(x => new { x.ProvinceId, x.AddressLine1, x.AddressLine2,
                        x.City, x.PostalCode }).FirstOrDefaultAsync(cancellationToken);
                var model = await DecorateAsync(new RegistrationAddressInput
                {
                    ProvinceId = address?.ProvinceId,
                    AddressLine1 = address?.AddressLine1,
                    AddressLine2 = address?.AddressLine2,
                    City = address?.City,
                    PostalCode = address?.PostalCode,
                }, app, key, 4, "Registered office address", cancellationToken);
                await PopulateProvincesAsync(model, cancellationToken);
                return View("Address", model);
            }
            case "summary":
                return RedirectToAction(nameof(Summary), new { id });
            default:
                return NotFound();
        }
    }

    // B2.5 — year-end month; separate from the B1 business-profile confirmation.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveYearEnd(RegistrationYearEndInput model,
        bool continueToNext, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(model.ApplicationId, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id = model.ApplicationId });
        if (model.YearEndMonth.HasValue && (model.YearEndMonth < 1 || model.YearEndMonth > 12))
            ModelState.AddModelError(nameof(model.YearEndMonth), "Choose a valid month.");
        if (continueToNext && !model.YearEndMonth.HasValue)
            ModelState.AddModelError(nameof(model.YearEndMonth), "Select a financial year-end month.");
        if (!ModelState.IsValid)
        {
            await DecorateAsync(model, app, "year-end", 5, "Financial year-end", cancellationToken);
            return View("YearEnd", model);
        }
        if (model.YearEndMonth.HasValue)
        {
            var existing = await _db.RegistrationFinancialYearEnds
                .SingleOrDefaultAsync(x => x.RegistrationApplicationId == app.RegistrationApplicationId,
                    cancellationToken);
            if (existing is null)
                _db.RegistrationFinancialYearEnds.Add(new RegistrationFinancialYearEnd
                {
                    RegistrationApplicationId = app.RegistrationApplicationId,
                    YearEndMonth = model.YearEndMonth.Value
                });
            else
                existing.YearEndMonth = model.YearEndMonth.Value;
        }
        return await SaveWizardProgressAsync(app, "year-end", continueToNext, cancellationToken);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNames(RegistrationNamesInput model,
        bool continueToNext, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(model.ApplicationId, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id = model.ApplicationId });

        var names = new[] { model.Name1, model.Name2, model.Name3 }
            .Select(x => (x ?? "").Trim()).ToArray();
        if (continueToNext && !model.ConfirmBusinessDetails)
            ModelState.AddModelError(nameof(model.ConfirmBusinessDetails),
                "Please confirm your business details before continuing.");
        if (continueToNext && names.All(string.IsNullOrWhiteSpace))
            ModelState.AddModelError("", "Enter at least one proposed name.");
        if (names.Any(n => n.Length > 150))
            ModelState.AddModelError("", "Each proposed name must be 150 characters or shorter.");
        if (names.Where(n => n.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Count()
            != names.Count(n => n.Length > 0))
            ModelState.AddModelError("", "Proposed names must be different.");
        // Numbered priorities must not have gaps; prevents hidden reordering.
        if ((names[0].Length == 0 && (names[1].Length > 0 || names[2].Length > 0)) ||
            (names[1].Length == 0 && names[2].Length > 0))
            ModelState.AddModelError("", "Enter names in order of preference, starting at 1.");

        if (!ModelState.IsValid)
        {
            var business = await _db.Businesses.AsNoTracking()
                .Where(b => b.BusinessId == app.BusinessId)
                .Select(b => new { b.TradingName, b.LegalName }).SingleAsync(cancellationToken);
            model.TradingName = business.TradingName;
            model.LegalName = business.LegalName;
            await DecorateAsync(model, app, "proposed-names", 1,
                "Proposed company names", cancellationToken);
            return View("ProposedNames", model);
        }

        var existing = await _db.RegistrationProposedNames
            .Where(x => x.RegistrationApplicationId == app.RegistrationApplicationId)
            .ToListAsync(cancellationToken);
        // Update rows in place to avoid unique (application, priority) collisions.
        for (var i = 0; i < 3; i++)
        {
            var order = i + 1;
            var row = existing.FirstOrDefault(x => x.PriorityOrder == order);
            var name = names[i];
            if (name.Length == 0)
            {
                if (row is not null) _db.RegistrationProposedNames.Remove(row);
            }
            else if (row is null)
            {
                _db.RegistrationProposedNames.Add(new RegistrationProposedName
                {
                    RegistrationProposedNameId = Guid.NewGuid(),
                    RegistrationApplicationId = app.RegistrationApplicationId,
                    PriorityOrder = order,
                    ProposedName = name,
                    AvailabilityStatus = "Pending"
                });
            }
            else if (!string.Equals(row.ProposedName, name, StringComparison.Ordinal))
            {
                row.ProposedName = name;
                row.AvailabilityStatus = "Pending";
            }
        }
        return await SaveWizardProgressAsync(app, "proposed-names", continueToNext, cancellationToken);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDirectors(RegistrationDirectorsInput model,
        bool continueToNext, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(model.ApplicationId, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id = model.ApplicationId });

        model.Directors ??= new();
        if (model.Directors.Count > 10)
            ModelState.AddModelError("", "Up to 10 directors are supported in this form.");
        var populated = model.Directors.Where(x => x.Id.HasValue ||
            !string.IsNullOrWhiteSpace(x.FullName) ||
            !string.IsNullOrWhiteSpace(x.Email) ||
            !string.IsNullOrWhiteSpace(x.ContactNumber)).ToList();
        if (continueToNext && populated.Count == 0)
            ModelState.AddModelError("", "Add at least one director.");
        foreach (var input in populated)
        {
            if (string.IsNullOrWhiteSpace(input.FullName) || input.FullName.Trim().Length > 150)
                ModelState.AddModelError("", "Each director needs a full name (maximum 150 characters).");
            if (string.IsNullOrWhiteSpace(input.Email) || input.Email.Length > 254 ||
                !new EmailAddressAttribute().IsValid(input.Email))
                ModelState.AddModelError("", "Each director needs a valid email address.");
            if (string.IsNullOrWhiteSpace(input.ContactNumber) || input.ContactNumber.Length > 30)
                ModelState.AddModelError("", "Each director needs a contact number (maximum 30 characters).");
        }
        if (populated.Where(x => x.Id.HasValue).Select(x => x.Id).Distinct().Count() !=
            populated.Count(x => x.Id.HasValue))
            ModelState.AddModelError("", "A director cannot be listed twice with the same record ID.");

        var existing = await _db.RegistrationDirectors
            .Where(x => x.RegistrationApplicationId == app.RegistrationApplicationId)
            .ToListAsync(cancellationToken);
        if (populated.Any(x => x.Id.HasValue && existing.All(y => y.RegistrationDirectorId != x.Id.Value)))
            ModelState.AddModelError("", "Invalid director record. Refresh and try again.");

        if (!ModelState.IsValid)
        {
            await DecorateAsync(model, app, "directors", 2, "Company directors", cancellationToken);
            return View("Directors", model);
        }
        foreach (var input in populated)
        {
            var record = input.Id.HasValue
                ? existing.Single(x => x.RegistrationDirectorId == input.Id.Value)
                : new RegistrationDirector
                {
                    RegistrationDirectorId = Guid.NewGuid(),
                    RegistrationApplicationId = app.RegistrationApplicationId,
                    RoleTitle = "Director"
                };
            record.FullName = input.FullName!.Trim();
            record.Email = input.Email!.Trim();
            record.ContactNumber = input.ContactNumber!.Trim();
            // Do not overwrite any previously encrypted identity value. We do not collect raw SA IDs in B2.
            if (!input.Id.HasValue) _db.RegistrationDirectors.Add(record);
        }
        var keep = populated.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        _db.RegistrationDirectors.RemoveRange(existing.Where(x => !keep.Contains(x.RegistrationDirectorId)));
        return await SaveWizardProgressAsync(app, "directors", continueToNext, cancellationToken);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveShareholders(RegistrationShareholdersInput model,
        bool continueToNext, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(model.ApplicationId, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id = model.ApplicationId });

        model.Shareholders ??= new();
        if (model.Shareholders.Count > 10)
            ModelState.AddModelError("", "Up to 10 shareholders are supported in this form.");
        var populated = model.Shareholders.Where(x => x.Id.HasValue ||
            !string.IsNullOrWhiteSpace(x.DisplayName) || x.SharePercentage.HasValue).ToList();
        if (continueToNext && populated.Count == 0)
            ModelState.AddModelError("", "Add at least one shareholder.");
        foreach (var input in populated)
        {
            if (string.IsNullOrWhiteSpace(input.DisplayName) || input.DisplayName.Trim().Length > 150)
                ModelState.AddModelError("", "Each shareholder needs a name (maximum 150 characters).");
            if (input.ShareholderType is not ("Individual" or "Company"))
                ModelState.AddModelError("", "Choose Individual or Company for each shareholder.");
            if (input.SharePercentage is null or <= 0 or > 100)
                ModelState.AddModelError("", "Every share percentage must be greater than 0 and at most 100.");
            else if (decimal.Round(input.SharePercentage.Value, 4) != input.SharePercentage.Value)
                ModelState.AddModelError("", "Share percentages may have at most four decimal places.");
        }
        if (continueToNext && populated.Sum(x => x.SharePercentage ?? 0m) != 100m)
            ModelState.AddModelError("", "Total shareholding must equal 100% to continue.");
        if (populated.Where(x => x.Id.HasValue).Select(x => x.Id).Distinct().Count() !=
            populated.Count(x => x.Id.HasValue))
            ModelState.AddModelError("", "A shareholder record was submitted twice.");

        var existing = await _db.RegistrationShareholders
            .Where(x => x.RegistrationApplicationId == app.RegistrationApplicationId)
            .ToListAsync(cancellationToken);
        if (populated.Any(x => x.Id.HasValue && existing.All(y => y.RegistrationShareholderId != x.Id.Value)))
            ModelState.AddModelError("", "Invalid shareholder record. Refresh and try again.");

        if (!ModelState.IsValid)
        {
            await DecorateAsync(model, app, "shareholders", 3, "Shareholding", cancellationToken);
            return View("Shareholders", model);
        }
        foreach (var input in populated)
        {
            var record = input.Id.HasValue
                ? existing.Single(x => x.RegistrationShareholderId == input.Id.Value)
                : new RegistrationShareholder
                {
                    RegistrationShareholderId = Guid.NewGuid(),
                    RegistrationApplicationId = app.RegistrationApplicationId,
                    ShareClass = "Ordinary"
                };
            record.DisplayName = input.DisplayName!.Trim();
            record.ShareholderType = input.ShareholderType!;
            record.SharePercentage = input.SharePercentage!.Value;
            // Preserve existing encrypted identity data (if previously supplied by a secure workflow).
            if (!input.Id.HasValue) _db.RegistrationShareholders.Add(record);
        }
        var keep = populated.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        _db.RegistrationShareholders.RemoveRange(existing.Where(x => !keep.Contains(x.RegistrationShareholderId)));
        return await SaveWizardProgressAsync(app, "shareholders", continueToNext, cancellationToken);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddress(RegistrationAddressInput model,
        bool continueToNext, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(model.ApplicationId, cancellationToken);
        if (app is null) return NotFound();
        if (!CanEditRegistration(app)) return RedirectToAction(nameof(Details), new { id = model.ApplicationId });

        var hasAny = (model.ProvinceId.HasValue && model.ProvinceId.Value != Guid.Empty) ||
            !string.IsNullOrWhiteSpace(model.AddressLine1) ||
            !string.IsNullOrWhiteSpace(model.AddressLine2) ||
            !string.IsNullOrWhiteSpace(model.City) ||
            !string.IsNullOrWhiteSpace(model.PostalCode);
        if (continueToNext || hasAny)
        {
            if (!model.ProvinceId.HasValue || model.ProvinceId.Value == Guid.Empty ||
                !await _db.ReferenceValues.AsNoTracking().AnyAsync(x =>
                    x.ReferenceValueId == model.ProvinceId && x.Category == "Province" && x.IsActive,
                    cancellationToken))
                ModelState.AddModelError(nameof(model.ProvinceId), "Select a valid province.");
            if (string.IsNullOrWhiteSpace(model.AddressLine1) || model.AddressLine1.Length > 200)
                ModelState.AddModelError(nameof(model.AddressLine1), "Enter a street address (maximum 200 characters).");
            if (model.AddressLine2?.Length > 200)
                ModelState.AddModelError(nameof(model.AddressLine2), "Address line 2 is too long.");
            if (string.IsNullOrWhiteSpace(model.City) || model.City.Length > 100)
                ModelState.AddModelError(nameof(model.City), "Enter a city or town (maximum 100 characters).");
            if (string.IsNullOrWhiteSpace(model.PostalCode) || model.PostalCode.Length > 12)
                ModelState.AddModelError(nameof(model.PostalCode), "Enter a valid postal code (maximum 12 characters).");
        }

        if (!ModelState.IsValid)
        {
            await DecorateAsync(model, app, "addresses", 4, "Registered office address", cancellationToken);
            await PopulateProvincesAsync(model, cancellationToken);
            return View("Address", model);
        }

        if (hasAny)
        {
            var address = await _db.RegistrationApplicationAddresses
                .FirstOrDefaultAsync(x => x.RegistrationApplicationId == app.RegistrationApplicationId &&
                    x.AddressType == "RegisteredOffice", cancellationToken);
            if (address is null)
            {
                address = new RegistrationApplicationAddress
                {
                    RegistrationApplicationAddressId = Guid.NewGuid(),
                    RegistrationApplicationId = app.RegistrationApplicationId,
                    AddressType = "RegisteredOffice"
                };
                _db.RegistrationApplicationAddresses.Add(address);
            }
            address.ProvinceId = model.ProvinceId!.Value;
            address.AddressLine1 = model.AddressLine1!.Trim();
            address.AddressLine2 = string.IsNullOrWhiteSpace(model.AddressLine2)
                ? null : model.AddressLine2.Trim();
            address.City = model.City!.Trim();
            address.PostalCode = model.PostalCode!.Trim();
        }
        return await SaveWizardProgressAsync(app, "addresses", continueToNext, cancellationToken);
    }

    private async Task PopulateProvincesAsync(RegistrationAddressInput model, CancellationToken ct)
    {
        model.Provinces = await _db.ReferenceValues.AsNoTracking()
            .Where(x => x.Category == "Province" && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.ReferenceValueId.ToString(), Text = x.Name
            }).ToListAsync(ct);
    }

    private async Task<IActionResult> SaveWizardProgressAsync(RegistrationApplication app,
        string key, bool continueToNext, CancellationToken ct)
    {
        var task = await _db.RegistrationApplicationTasks.SingleOrDefaultAsync(x =>
            x.RegistrationApplicationId == app.RegistrationApplicationId && x.TaskKey == key, ct);
        if (task is null) return StatusCode(409, "This registration step was not created with the application.");

        var now = DateTime.UtcNow;
        if (continueToNext)
        {
            // B1's first checklist entry is confirmation of the existing business profile.
            // The names form asks for that confirmation before marking it complete.
            if (key == "proposed-names")
            {
                var businessTask = await _db.RegistrationApplicationTasks.SingleOrDefaultAsync(t =>
                    t.RegistrationApplicationId == app.RegistrationApplicationId &&
                    t.TaskKey == "business-details", ct);
                if (businessTask is not null)
                {
                    businessTask.Status = "Completed";
                    businessTask.CompletedAtUtc = now;
                }
            }
            task.Status = "Completed";
            task.CompletedAtUtc = now;
            var next = NextKey(key)!;
            var nextTask = await _db.RegistrationApplicationTasks.SingleOrDefaultAsync(x =>
                x.RegistrationApplicationId == app.RegistrationApplicationId && x.TaskKey == next, ct);
            if (nextTask is not null && nextTask.Status == "Pending")
                nextTask.Status = "InProgress";

            // Do not regress the resume marker when editing a previously completed step.
            var position = Array.IndexOf(EditableWizardKeys, key);
            var currentPosition = Array.IndexOf(EditableWizardKeys, app.CurrentStepKey);
            if (currentPosition < 0 || currentPosition <= position)
                app.CurrentStepKey = next;
        }
        else
        {
            task.Status = "InProgress";
            task.CompletedAtUtc = null;
            app.CurrentStepKey = key;
        }
        app.UpdatedAtUtc = now;

        if (TryGetUserId(out var userId))
        {
            _db.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(), ActorUserId = userId,
                BusinessId = app.BusinessId, OccurredAtUtc = now,
                ActionCode = "registration.step_saved",
                EntityType = "RegistrationApplication",
                EntityId = app.RegistrationApplicationId.ToString(),
                // No director names, phone numbers or address contents in audit JSON.
                PayloadJson = JsonSerializer.Serialize(new { step = key, completed = continueToNext })
            });
        }
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = continueToNext ? "Step completed and saved." : "Draft saved. You can resume later.";

        if (!continueToNext)
            return RedirectToAction(nameof(Details), new { id = app.RegistrationApplicationId });
        var nextKey = NextKey(key)!;
        return nextKey == "review"
            ? RedirectToAction(nameof(Summary), new { id = app.RegistrationApplicationId })
            : RedirectToAction(nameof(Step), new { id = app.RegistrationApplicationId, key = nextKey });
    }

    // Read-only B2 review. Formal submission and reviewer decisions are implemented in B4.
    [HttpGet]
    public async Task<IActionResult> Summary(Guid id, CancellationToken cancellationToken)
    {
        var app = await AccessibleApplicationAsync(id, cancellationToken);
        if (app is null) return NotFound();
        var businessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == app.BusinessId)
            .Select(b => b.TradingName).SingleAsync(cancellationToken);
        var address = await _db.RegistrationApplicationAddresses.AsNoTracking()
            .Where(x => x.RegistrationApplicationId == id && x.AddressType == "RegisteredOffice")
            .Select(x => new { x.AddressLine1, x.City, x.PostalCode, x.ProvinceId })
            .FirstOrDefaultAsync(cancellationToken);
        var province = address is null ? null : await _db.ReferenceValues.AsNoTracking()
            .Where(x => x.ReferenceValueId == address.ProvinceId)
            .Select(x => x.Name).FirstOrDefaultAsync(cancellationToken);
        var model = new RegistrationWizardSummaryViewModel
        {
            ApplicationId = id,
            BusinessId = app.BusinessId,
            BusinessName = businessName,
            StepKey = "review",
            StepNumber = 6,
            StepTitle = "Review your registration information",
            Status = app.Status,
            YearEndMonth = await _db.RegistrationFinancialYearEnds.AsNoTracking()
                .Where(x => x.RegistrationApplicationId == id)
                .Select(x => (int?)x.YearEndMonth).FirstOrDefaultAsync(cancellationToken),
            ProposedNames = await _db.RegistrationProposedNames.AsNoTracking()
                .Where(x => x.RegistrationApplicationId == id).OrderBy(x => x.PriorityOrder)
                .Select(x => x.ProposedName).ToListAsync(cancellationToken),
            DirectorNames = await _db.RegistrationDirectors.AsNoTracking()
                .Where(x => x.RegistrationApplicationId == id).OrderBy(x => x.FullName)
                .Select(x => x.FullName).ToListAsync(cancellationToken),
            Shareholders = await _db.RegistrationShareholders.AsNoTracking()
                .Where(x => x.RegistrationApplicationId == id)
                .OrderBy(x => x.DisplayName)
                .Select(x => new RegistrationShareholderSummary
                {
                    Name = x.DisplayName, Percentage = (decimal)x.SharePercentage
                }).ToListAsync(cancellationToken),
            RegisteredAddress = address is null ? "Not entered" :
                string.Join(", ", new[] { address.AddressLine1, address.City, province, address.PostalCode }
                    .Where(x => !string.IsNullOrWhiteSpace(x))),
            Steps = await _db.RegistrationApplicationTasks.AsNoTracking()
                .Where(x => x.RegistrationApplicationId == id)
                .OrderBy(x => x.TaskKey)
                .Select(x => new RegistrationStepViewModel
                {
                    StepKey = x.TaskKey, Title = x.TitleSnapshot,
                    Instructions = x.InstructionsSnapshot, Status = x.Status
                }).ToListAsync(cancellationToken)
        };
        return View(model);
    }
}
