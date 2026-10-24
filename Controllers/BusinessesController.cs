
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.ViewModels;

// Add your actual Data and Models namespaces here.

[Authorize]
public class BusinessesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // BUSINESS CREATION
    [HttpGet]
    public async Task<IActionResult> CreateBusiness()
    {
        var model = new CreateBusinessViewModel();

        await PopulateDropdownsAsync(model);

        return View(model);
    }

    // POST: /Businesses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBusiness(
        CreateBusinessViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Confirm selected IDs are genuine lookup values.
            await ValidateSelectionsAsync(model);
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            return View(model);
        }

        var userId = Guid.Parse(
            _userManager.GetUserId(User)!);

        var now = DateTime.UtcNow;

        var business = new Business
        {
            BusinessId = Guid.NewGuid(),
            TradingName = model.TradingName.Trim(),
            LegalName = model.LegalName?.Trim(),
            RegistrationNumber = model.RegistrationNumber?.Trim(),
            BusinessTypeId = model.BusinessTypeId!.Value,
            BusinessStageId = model.BusinessStageId!.Value,
            SectorId = model.SectorId!.Value,
            ProvinceId = model.ProvinceId!.Value,
            CreatedByUserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var owner = new BusinessMember
        {
            BusinessMemberId = Guid.NewGuid(),
            BusinessId = business.BusinessId,
            UserId = userId,
            BusinessRole = "Owner",
            MembershipStatus = "Active",
            InvitedAtUtc = now
        };

        _db.Businesses.Add(business);
        _db.BusinessMembers.Add(owner);

        // One SaveChanges call saves both records
        // atomically using EF Core's default transaction.
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Your business has been created successfully!";

        return RedirectToAction(
            "Index",
            "Dashboard",
            new { businessId = business.BusinessId });
    }

    private async Task PopulateDropdownsAsync(
        CreateBusinessViewModel model)
    {
        model.BusinessTypes =
            await GetReferenceOptionsAsync("BusinessType");

        model.BusinessStages =
            await GetReferenceOptionsAsync("BusinessStage");

        model.Provinces =
            await GetReferenceOptionsAsync("Province");

        var sectors = await _db.Sectors
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new { s.SectorId, s.Name })
            .ToListAsync();

        model.Sectors = sectors
            .Select(s => new SelectListItem
            {
                Value = s.SectorId.ToString(),
                Text = s.Name
            })
            .ToList();
    }

    private async Task<List<SelectListItem>>
        GetReferenceOptionsAsync(string category)
    {
        var values = await _db.ReferenceValues
            .AsNoTracking()
            .Where(r => r.Category == category && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .Select(r => new { r.ReferenceValueId, r.Name })
            .ToListAsync();

        return values
            .Select(r => new SelectListItem
            {
                Value = r.ReferenceValueId.ToString(),
                Text = r.Name
            })
            .ToList();
    }

    private async Task ValidateSelectionsAsync(
        CreateBusinessViewModel model)
    {
        foreach (var item in new[]
        {
            (Id: model.BusinessTypeId!.Value,
             Category: "BusinessType",
             Property: nameof(model.BusinessTypeId)),

            (Id: model.BusinessStageId!.Value,
             Category: "BusinessStage",
             Property: nameof(model.BusinessStageId)),

            (Id: model.ProvinceId!.Value,
             Category: "Province",
             Property: nameof(model.ProvinceId))
        })
        {
            var exists = await _db.ReferenceValues.AnyAsync(r =>
                r.ReferenceValueId == item.Id &&
                r.Category == item.Category &&
                r.IsActive);

            if (!exists)
                ModelState.AddModelError(
                    item.Property, "Please select a valid option.");
        }

        var sectorExists = await _db.Sectors
            .AnyAsync(s => s.SectorId == model.SectorId!.Value);

        if (!sectorExists)
            ModelState.AddModelError(
                nameof(model.SectorId),
                "Please select a valid sector.");
    }

    // BUSINESS MANAGEMENT

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        var businesses = await AccessibleBusinessCards(userId)
            .OrderBy(b => b.TradingName)
            .ToListAsync();

        return View(new BusinessIndexViewModel { Businesses = businesses });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        var business = await AccessibleBusinessCards(userId)
            .FirstOrDefaultAsync(b => b.BusinessId == id);

        if (business == null)
            return NotFound();

        return View(business);
    }

    // Filtering by the current signed-in user's active membership prevents
    // users from viewing another business just by changing the URL ID.
    private IQueryable<BusinessCardViewModel> AccessibleBusinessCards(Guid userId)
    {
        return
            from membership in _db.BusinessMembers.AsNoTracking()
            join business in _db.Businesses.AsNoTracking()
                on membership.BusinessId equals business.BusinessId
            where membership.UserId == userId
               && membership.MembershipStatus == "Active"
            select new BusinessCardViewModel
            {
                BusinessId = business.BusinessId,
                TradingName = business.TradingName,
                LegalName = business.LegalName,
                RegistrationNumber = business.RegistrationNumber,
                CreatedAtUtc = business.CreatedAtUtc,
                Role = membership.BusinessRole,
                BusinessType = _db.ReferenceValues
                    .Where(r => r.ReferenceValueId == business.BusinessTypeId
                             && r.Category == "BusinessType")
                    .Select(r => r.Name).FirstOrDefault() ?? "Not specified",
                BusinessStage = _db.ReferenceValues
                    .Where(r => r.ReferenceValueId == business.BusinessStageId
                             && r.Category == "BusinessStage")
                    .Select(r => r.Name).FirstOrDefault() ?? "Not specified",
                Province = _db.ReferenceValues
                    .Where(r => r.ReferenceValueId == business.ProvinceId
                             && r.Category == "Province")
                    .Select(r => r.Name).FirstOrDefault() ?? "Not specified",
                Sector = _db.Sectors
                    .Where(s => s.SectorId == business.SectorId)
                    .Select(s => s.Name).FirstOrDefault() ?? "Not specified"
            };
    }

    // EDIT BUSINESS
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        if (!await CanEditBusinessAsync(id, userId))
            return NotFound();

        var business = await _db.Businesses.AsNoTracking()
            .FirstOrDefaultAsync(b => b.BusinessId == id);
        if (business == null)
            return NotFound();

        var model = new EditBusinessViewModel
        {
            BusinessId = business.BusinessId,
            TradingName = business.TradingName,
            LegalName = business.LegalName,
            RegistrationNumber = business.RegistrationNumber,
            BusinessTypeId = business.BusinessTypeId,
            BusinessStageId = business.BusinessStageId,
            SectorId = business.SectorId,
            ProvinceId = business.ProvinceId
        };

        await PopulateEditDropdownsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditBusinessViewModel model)
    {
        if (id != model.BusinessId || id == Guid.Empty)
            return BadRequest();

        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        if (!await CanEditBusinessAsync(id, userId))
            return NotFound();

        if (ModelState.IsValid)
            await ValidateEditSelectionsAsync(model);

        if (!ModelState.IsValid)
        {
            await PopulateEditDropdownsAsync(model);
            return View(model);
        }

        var business = await _db.Businesses
            .FirstOrDefaultAsync(b => b.BusinessId == id);
        if (business == null)
            return NotFound();

        // Update only allowed fields. Never bind an EF Business directly.
        business.TradingName = model.TradingName.Trim();
        business.LegalName = string.IsNullOrWhiteSpace(model.LegalName)
            ? null : model.LegalName.Trim();
        business.RegistrationNumber = string.IsNullOrWhiteSpace(model.RegistrationNumber)
            ? null : model.RegistrationNumber.Trim();
        business.BusinessTypeId = model.BusinessTypeId!.Value;
        business.BusinessStageId = model.BusinessStageId!.Value;
        business.SectorId = model.SectorId!.Value;
        business.ProvinceId = model.ProvinceId!.Value;
        business.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Business profile updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private Task<bool> CanEditBusinessAsync(Guid businessId, Guid userId)
    {
        return _db.BusinessMembers.AnyAsync(m =>
            m.BusinessId == businessId &&
            m.UserId == userId &&
            m.MembershipStatus == "Active" &&
            (m.BusinessRole == "Owner" || m.BusinessRole == "Manager"));
    }

    private async Task PopulateEditDropdownsAsync(EditBusinessViewModel model)
    {
        model.BusinessTypes = await GetEditReferenceOptionsAsync("BusinessType");
        model.BusinessStages = await GetEditReferenceOptionsAsync("BusinessStage");
        model.Provinces = await GetEditReferenceOptionsAsync("Province");

        var sectors = await _db.Sectors.AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new { s.SectorId, s.Name })
            .ToListAsync();

        model.Sectors = sectors.Select(s => new SelectListItem
        {
            Value = s.SectorId.ToString(),
            Text = s.Name
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetEditReferenceOptionsAsync(string category)
    {
        var values = await _db.ReferenceValues.AsNoTracking()
            .Where(r => r.Category == category && r.IsActive)
            .OrderBy(r => r.SortOrder)
            .Select(r => new { r.ReferenceValueId, r.Name })
            .ToListAsync();

        return values.Select(r => new SelectListItem
        {
            Value = r.ReferenceValueId.ToString(),
            Text = r.Name
        }).ToList();
    }

    private async Task ValidateEditSelectionsAsync(EditBusinessViewModel model)
    {
        var referenceSelections = new[]
        {
        (Id: model.BusinessTypeId!.Value, Category: "BusinessType", Property: nameof(model.BusinessTypeId)),
        (Id: model.BusinessStageId!.Value, Category: "BusinessStage", Property: nameof(model.BusinessStageId)),
        (Id: model.ProvinceId!.Value, Category: "Province", Property: nameof(model.ProvinceId))
    };

        foreach (var item in referenceSelections)
        {
            bool valid = await _db.ReferenceValues.AnyAsync(r =>
                r.ReferenceValueId == item.Id &&
                r.Category == item.Category && r.IsActive);
            if (!valid)
                ModelState.AddModelError(item.Property, "Select a valid option.");
        }

        if (!await _db.Sectors.AnyAsync(s => s.SectorId == model.SectorId!.Value))
            ModelState.AddModelError(nameof(model.SectorId), "Select a valid sector.");
    }

}
