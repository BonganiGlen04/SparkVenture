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
using SparkVenture.Services;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class TenderApplicationsController : OpportunityControllerBase
{
    public TenderApplicationsController(ApplicationDbContext db, UserManager<ApplicationUser> users) : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        if (businessId is null) {
            var id = await SingleBusinessAsync(userId, ct);
            return id is Guid found ? RedirectToAction(nameof(Index), new { businessId = found }) : ChooseBusiness();
        }
        var access = await AccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();
        var items = await (from app in Db.TenderApplications.AsNoTracking()
                           join opportunity in Db.Opportunities.AsNoTracking()
                              on app.OpportunityId equals opportunity.OpportunityId
                           where app.BusinessId == businessId.Value
                           orderby app.SubmissionDeadlineUtc
                           select new TenderRowVm {
                               TenderApplicationId = app.TenderApplicationId,
                               OpportunityId = app.OpportunityId, Title = opportunity.Title,
                               Status = app.Status, DeadlineUtc = app.SubmissionDeadlineUtc,
                               SubmittedAtUtc = app.SubmittedAtUtc
                           }).Take(150).ToListAsync(ct);
        return View(new TenderApplicationsIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name,
            CanManage = access.CanManage, Items = items
        });
    }

    // Creates a LOCAL preparation record; no external tender filing occurs here.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid businessId, Guid opportunityId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        await using var tx = await Db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await Db.TenderApplications.FirstOrDefaultAsync(x =>
            x.BusinessId == businessId && x.OpportunityId == opportunityId, ct);
        if (existing is not null) {
            await tx.CommitAsync(ct);
            return RedirectToAction(nameof(Details), new { businessId, id = existing.TenderApplicationId });
        }
        var opportunity = await Db.Opportunities.AsNoTracking().SingleOrDefaultAsync(x =>
            x.OpportunityId == opportunityId && x.OpportunityType == "Tender" &&
            x.Status == "Open" && x.ClosesAtUtc > DateTime.UtcNow &&
            (x.OpensAtUtc == null || x.OpensAtUtc <= DateTime.UtcNow), ct);
        if (opportunity is null) return BadRequest("This tender is not currently open for preparation.");
        var id = Guid.NewGuid();
        Db.TenderApplications.Add(new TenderApplication {
            TenderApplicationId = id, BusinessId = businessId,
            OpportunityId = opportunityId, CreatedByUserId = userId,
            Status = "InProgress", SubmissionDeadlineUtc = opportunity.ClosesAtUtc
        });
        Audit(userId, businessId, "tender.preparation_started", "TenderApplication", id);
        await Db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return RedirectToAction(nameof(Details), new { businessId, id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid businessId, Guid id, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null) return NotFound();
        var record = await (from app in Db.TenderApplications.AsNoTracking()
                            join opportunity in Db.Opportunities.AsNoTracking()
                                on app.OpportunityId equals opportunity.OpportunityId
                            join source in Db.OpportunitySources.AsNoTracking()
                                on opportunity.OpportunitySourceId equals source.OpportunitySourceId
                            where app.TenderApplicationId == id && app.BusinessId == businessId
                            select new {
                                app.TenderApplicationId, app.OpportunityId, app.Status,
                                app.SubmissionDeadlineUtc, app.SubmittedAtUtc,
                                opportunity.Title, opportunity.ExternalId,
                                source.Name, source.WebsiteUrl
                            }).SingleOrDefaultAsync(ct);
        if (record is null) return NotFound();
        var requirements = await Db.OpportunityRequirements.AsNoTracking()
            .Where(x => x.OpportunityId == record.OpportunityId)
            .OrderByDescending(x => x.IsMandatory)
            .Select(x => new OpportunityRequirementVm { Type = x.RequirementType,
                Operator = x.Operator, Value = x.Value, IsMandatory = x.IsMandatory })
            .ToListAsync(ct);
        var docs = await (from link in Db.TenderApplicationDocuments.AsNoTracking()
                          join document in Db.Documents.AsNoTracking()
                              on link.DocumentId equals document.DocumentId
                          where link.TenderApplicationId == id && link.BusinessId == businessId &&
                              document.BusinessId == businessId
                          orderby link.CreatedAtUtc descending
                          select new TenderDocumentRowVm {
                              TenderApplicationDocumentId = link.TenderApplicationDocumentId,
                              OriginalFileName = document.OriginalFileName,
                              Purpose = link.DocumentPurpose,
                              HasStoredContent = Db.DocumentContents.Any(x => x.DocumentId == document.DocumentId)
                          }).ToListAsync(ct);
        var available = await (
            from link in Db.BusinessDocuments.AsNoTracking()
            join document in Db.Documents.AsNoTracking()
                on link.DocumentId equals document.DocumentId
            where link.BusinessId == businessId && document.BusinessId == businessId &&
                Db.DocumentContents.Any(c => c.DocumentId == document.DocumentId) &&
                !Db.TenderApplicationDocuments.Any(x => x.BusinessId == businessId &&
                    x.TenderApplicationId == id && x.DocumentId == document.DocumentId)
            orderby document.OriginalFileName
            select new SelectListItem {
                Value = document.DocumentId.ToString(), Text = document.OriginalFileName
            }).Take(100).ToListAsync(ct);
        return View(new TenderDetailsVm {
            BusinessId = businessId, BusinessName = access.Name, CanManage = access.CanManage,
            Application = new TenderRowVm {
                TenderApplicationId = record.TenderApplicationId,
                OpportunityId = record.OpportunityId, Title = record.Title, Status = record.Status,
                DeadlineUtc = record.SubmissionDeadlineUtc, SubmittedAtUtc = record.SubmittedAtUtc
            },
            SourceName = record.Name, SourceWebsite = record.WebsiteUrl,
            IsDemo = (record.ExternalId ?? "").StartsWith("SEED-", StringComparison.OrdinalIgnoreCase),
            Requirements = requirements, Documents = docs, AvailableDocuments = available
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AttachDocument(Guid businessId, Guid tenderApplicationId,
        Guid documentId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        var app = await Db.TenderApplications.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId &&
            x.Status == "InProgress", ct);
        if (app is null) return NotFound();
        bool inVault = await (from link in Db.BusinessDocuments.AsNoTracking()
                             join document in Db.Documents.AsNoTracking()
                               on link.DocumentId equals document.DocumentId
                             where link.BusinessId == businessId && document.BusinessId == businessId &&
                                 document.DocumentId == documentId &&
                                 Db.DocumentContents.Any(x => x.DocumentId == documentId)
                             select link.BusinessDocumentId).AnyAsync(ct);
        if (!inVault) return BadRequest("Choose a stored document from this business vault.");
        if (!await Db.TenderApplicationDocuments.AnyAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId && x.DocumentId == documentId, ct)) {
            var linkId = Guid.NewGuid();
            Db.TenderApplicationDocuments.Add(new TenderApplicationDocument {
                TenderApplicationDocumentId = linkId, TenderApplicationId = tenderApplicationId,
                DocumentId = documentId, BusinessId = businessId,
                DocumentPurpose = "SupportingEvidence", IsRequiredEvidence = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            Audit(userId, businessId, "tender.document_linked", "TenderApplicationDocument", linkId);
            await Db.SaveChangesAsync(ct);
        }
        return RedirectToAction(nameof(Details), new { businessId, id = tenderApplicationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDocument(Guid businessId, Guid tenderApplicationId,
        Guid tenderApplicationDocumentId, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (!await Db.TenderApplications.AnyAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId &&
            x.Status == "InProgress", ct)) return NotFound();
        var link = await Db.TenderApplicationDocuments.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId &&
            x.TenderApplicationDocumentId == tenderApplicationDocumentId, ct);
        if (link is null) return NotFound();
        Db.TenderApplicationDocuments.Remove(link);
        Audit(userId, businessId, "tender.document_unlinked", "TenderApplicationDocument", tenderApplicationDocumentId);
        await Db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Details), new { businessId, id = tenderApplicationId });
    }

    // This records the founder's confirmation of an EXTERNAL submission.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportSubmitted(Guid businessId, Guid tenderApplicationId,
        bool confirmedExternalSubmission, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (!confirmedExternalSubmission) return BadRequest("Confirm that you submitted externally.");
        var record = await Db.TenderApplications.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId &&
            x.Status == "InProgress", ct);
        if (record is null) return NotFound();
        // External submissions should be recorded even after the local deadline.
        record.Status = "Submitted";
        record.SubmittedAtUtc = DateTime.UtcNow;
        Audit(userId, businessId, "tender.externally_submitted_self_reported", "TenderApplication", tenderApplicationId);
        await Db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Recorded as submitted externally (self-reported; not independently verified).";
        return RedirectToAction(nameof(Details), new { businessId, id = tenderApplicationId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportOutcome(Guid businessId, Guid tenderApplicationId,
        string outcome, CancellationToken ct)
    {
        if (!TryUser(out var userId)) return Challenge();
        var access = await AccessAsync(businessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();
        if (outcome is not ("Awarded" or "Unsuccessful" or "Withdrawn")) return BadRequest();
        var record = await Db.TenderApplications.SingleOrDefaultAsync(x =>
            x.BusinessId == businessId && x.TenderApplicationId == tenderApplicationId &&
            x.Status == "Submitted", ct);
        if (record is null) return NotFound();
        record.Status = outcome;
        Audit(userId, businessId, "tender.outcome_self_reported", "TenderApplication", tenderApplicationId);
        await Db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Outcome saved as self-reported; not independently verified.";
        return RedirectToAction(nameof(Details), new { businessId, id = tenderApplicationId });
    }
}
