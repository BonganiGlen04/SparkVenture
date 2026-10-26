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
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class FundingApplicationsController : FundingControllerBase
{
    public FundingApplicationsController(ApplicationDbContext db, UserManager<ApplicationUser> users)
        : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!businessId.HasValue)
        {
            var selected = await GetSingleBusinessIdAsync(userId, ct);
            return selected.HasValue ? RedirectToAction(nameof(Index), new { businessId = selected.Value }) : PickBusiness();
        }
        var access = await GetAccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var applications = await (from a in Db.FundingApplications.AsNoTracking()
                join provider in Db.FundingProviders.AsNoTracking() on (Guid?)a.FundingProviderId equals (Guid?)provider.FundingProviderId
                where a.BusinessId == businessId.Value
                orderby a.CreatedAtUtc descending
                select new FundingApplicationRowVm {
                    FundingApplicationId = a.FundingApplicationId,
                    FundingProviderId = a.FundingProviderId,
                    ProviderName = provider.Name, Status = a.Status,
                    RequestedAmount = (decimal)a.RequestedAmount,
                    CreatedAtUtc = a.CreatedAtUtc
                }).Take(150).ToListAsync(ct);
        return View(new FundingApplicationsIndexVm { BusinessId = businessId.Value,
            BusinessName = access.Name, CanManage = access.CanManage, Applications = applications });
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid businessId, Guid? providerId, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        var model = new FundingApplicationCreateVm { BusinessId = businessId,
            BusinessName = access.Name, FundingProviderId = providerId ?? Guid.Empty };
        await PopulateProvidersAsync(model, ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FundingApplicationCreateVm model, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        model.BusinessName = access.Name;
        await PopulateProvidersAsync(model, ct);
        if (!await Db.FundingProviders.AsNoTracking().AnyAsync(x => x.FundingProviderId == model.FundingProviderId && x.IsActive, ct))
            ModelState.AddModelError(nameof(model.FundingProviderId), "Choose an active provider.");
        if (!ModelState.IsValid) return View(model);

        var id = Guid.NewGuid();
        Db.FundingApplications.Add(new FundingApplication {
            FundingApplicationId = id, BusinessId = model.BusinessId,
            OpportunityId = null, FundingProviderId = model.FundingProviderId,
            CreatedByUserId = userId, Status = "Draft",
            RequestedAmount = model.RequestedAmount, CurrencyCode = "ZAR",
            CreatedAtUtc = DateTime.UtcNow
        });
        AddAudit(userId, model.BusinessId, "funding.application_drafted", "FundingApplication", id);
        await Db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Details), new { businessId = model.BusinessId, id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var record = await (from app in Db.FundingApplications.AsNoTracking()
                join provider in Db.FundingProviders.AsNoTracking() on (Guid?)app.FundingProviderId equals (Guid?)provider.FundingProviderId
                where app.BusinessId == businessId && app.FundingApplicationId == id
                select new { app.FundingApplicationId, app.FundingProviderId,
                    app.RequestedAmount, app.CreatedAtUtc, app.Status,
                    ProviderName = provider.Name, provider.WebsiteUrl
                }).SingleOrDefaultAsync(ct);
        if (record is null) return NotFound();

        var documents = await (from link in Db.FundingApplicationDocuments.AsNoTracking()
                join document in Db.Documents.AsNoTracking() on link.DocumentId equals document.DocumentId
                where link.BusinessId == businessId && link.FundingApplicationId == id &&
                      document.BusinessId == businessId
                orderby link.CreatedAtUtc descending
                select new FundingApplicationDocumentVm {
                    FundingApplicationDocumentId = link.FundingApplicationDocumentId,
                    FileName = document.OriginalFileName,
                    Purpose = link.DocumentPurpose
                }).ToListAsync(ct);
        var choices = await (from vault in Db.BusinessDocuments.AsNoTracking()
                join doc in Db.Documents.AsNoTracking() on vault.DocumentId equals doc.DocumentId
                where vault.BusinessId == businessId && doc.BusinessId == businessId &&
                      Db.DocumentContents.Any(c => c.DocumentId == doc.DocumentId) &&
                      !Db.FundingApplicationDocuments.Any(link => link.BusinessId == businessId &&
                          link.FundingApplicationId == id && link.DocumentId == doc.DocumentId)
                orderby doc.OriginalFileName
                select new SelectListItem { Value = doc.DocumentId.ToString(), Text = doc.OriginalFileName })
            .Take(100).ToListAsync(ct);
        string? safeWebsite = Uri.TryCreate(record.WebsiteUrl, UriKind.Absolute, out var url) &&
                              url.Scheme == Uri.UriSchemeHttps && !url.IsLoopback && string.IsNullOrEmpty(url.UserInfo)
                              ? record.WebsiteUrl : null;
        return View(new FundingApplicationDetailsVm {
            BusinessId = businessId, BusinessName = access.Name, CanManage = access.CanManage,
            Application = new FundingApplicationRowVm {
                FundingApplicationId = record.FundingApplicationId, FundingProviderId = record.FundingProviderId,
                ProviderName = record.ProviderName, RequestedAmount = (decimal)record.RequestedAmount,
                CreatedAtUtc = record.CreatedAtUtc, Status = record.Status
            },
            IsDemoProvider = (record.WebsiteUrl ?? "").Contains("example.org", StringComparison.OrdinalIgnoreCase),
            WebsiteUrl = safeWebsite, Documents = documents, AvailableDocuments = choices
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AttachDocument(Guid businessId, Guid fundingApplicationId,
        Guid documentId, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        var draft = await Db.FundingApplications.AsNoTracking().AnyAsync(x =>
            x.BusinessId == businessId && x.FundingApplicationId == fundingApplicationId && x.Status == "Draft", ct);
        if (!draft) return NotFound();
        var belongsToVault = await (from link in Db.BusinessDocuments.AsNoTracking()
                join document in Db.Documents.AsNoTracking() on link.DocumentId equals document.DocumentId
                where link.BusinessId == businessId && document.BusinessId == businessId &&
                      document.DocumentId == documentId &&
                      Db.DocumentContents.Any(c => c.DocumentId == documentId)
                select link.BusinessDocumentId).AnyAsync(ct);
        if (!belongsToVault) return BadRequest("Choose a stored file from this business's vault.");
        if (!await Db.FundingApplicationDocuments.AnyAsync(x => x.BusinessId == businessId &&
            x.FundingApplicationId == fundingApplicationId && x.DocumentId == documentId, ct))
        {
            var id = Guid.NewGuid();
            Db.FundingApplicationDocuments.Add(new FundingApplicationDocument {
                FundingApplicationDocumentId = id, FundingApplicationId = fundingApplicationId,
                DocumentId = documentId, BusinessId = businessId,
                DocumentPurpose = "SupportingEvidence", IsRequiredEvidence = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            AddAudit(userId, businessId, "funding.document_linked", "FundingApplicationDocument", id);
            await Db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Details), new { businessId, id = fundingApplicationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDocument(Guid businessId, Guid fundingApplicationId,
        Guid fundingApplicationDocumentId, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (!await Db.FundingApplications.AsNoTracking().AnyAsync(x => x.BusinessId == businessId &&
                x.FundingApplicationId == fundingApplicationId && x.Status == "Draft", ct)) return NotFound();
        var link = await Db.FundingApplicationDocuments.SingleOrDefaultAsync(x =>
            x.FundingApplicationDocumentId == fundingApplicationDocumentId &&
            x.FundingApplicationId == fundingApplicationId && x.BusinessId == businessId, ct);
        if (link is null) return NotFound();
        Db.FundingApplicationDocuments.Remove(link); // Underlying document bytes are retained.
        AddAudit(userId, businessId, "funding.document_unlinked", "FundingApplicationDocument", link.FundingApplicationDocumentId);
        await Db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Details), new { businessId, id = fundingApplicationId });
    }

    // Records only a founder's claim about an external submission. Does NOT send data to providers.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportSubmitted(Guid businessId, Guid fundingApplicationId,
        bool confirmedExternalSubmission, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (!confirmedExternalSubmission) return BadRequest("Confirm the submission happened outside SparkVenture.");
        // Never allow seeded example.org demo providers to be marked as actually submitted.
        var demoProvider = await (from application in Db.FundingApplications.AsNoTracking()
                    join provider in Db.FundingProviders.AsNoTracking()
                on (Guid?)application.FundingProviderId equals (Guid?)provider.FundingProviderId
            where application.FundingApplicationId == fundingApplicationId && application.BusinessId == businessId
            select provider.WebsiteUrl).SingleOrDefaultAsync(ct);
        if ((demoProvider ?? "").Contains("example.org", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Demo providers cannot be marked as externally submitted.");
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var app = await Db.FundingApplications.SingleOrDefaultAsync(x => x.BusinessId == businessId &&
            x.FundingApplicationId == fundingApplicationId && x.Status == "Draft", ct);
        if (app is null) return NotFound();
        app.Status = "Submitted";
        AddAudit(userId, businessId, "funding.external_submission_reported", "FundingApplication", app.FundingApplicationId);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "External submission recorded as self-reported. SparkVenture did not submit it.";
        return RedirectToAction(nameof(Details), new { businessId, id = fundingApplicationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportOutcome(Guid businessId, Guid fundingApplicationId,
        string reportedStatus, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (reportedStatus != "UnderReview" && reportedStatus != "Approved" && reportedStatus != "Rejected")
            return BadRequest("Choose a supported outcome.");
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var app = await Db.FundingApplications.SingleOrDefaultAsync(x => x.BusinessId == businessId &&
            x.FundingApplicationId == fundingApplicationId &&
            (x.Status == "Submitted" || x.Status == "UnderReview"), ct);
        if (app is null) return NotFound();
        app.Status = reportedStatus;
        AddAudit(userId, businessId, "funding.external_status_reported", "FundingApplication", app.FundingApplicationId);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        TempData["SuccessMessage"] = "Self-reported status saved. Funders have not verified this update in SparkVenture.";
        return RedirectToAction(nameof(Details), new { businessId, id = fundingApplicationId });
    }

    private async Task PopulateProvidersAsync(FundingApplicationCreateVm model, CancellationToken ct)
    {
        model.ProviderOptions = await Db.FundingProviders.AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.Name)
            .Select(x => new SelectListItem { Value = x.FundingProviderId.ToString(), Text = x.Name })
            .ToListAsync(ct);
    }
}
