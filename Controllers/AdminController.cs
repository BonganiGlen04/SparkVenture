using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Infrastructure;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

// DO NOT replace the Admin Identity role with Owner/Manager business roles.
[Authorize(Roles = "Admin")]
public sealed class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db; _users = users;
    }
    private bool CurrentUser(out Guid id) => Guid.TryParse(_users.GetUserId(User), out id);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct) => View(new AdminOverviewVm
    {
        BusinessCount = await _db.Businesses.AsNoTracking().CountAsync(ct),
        UserCount = await _db.Users.AsNoTracking().CountAsync(ct),
        ReferenceCount = await _db.ReferenceValues.AsNoTracking().CountAsync(ct),
        PendingRequestCount = await _db.DataSubjectRequests.AsNoTracking()
            .CountAsync(x => x.Status == "Received" || x.Status == "InProgress", ct),
        AuditCount = await _db.AuditLogs.AsNoTracking().CountAsync(ct)
    });

    [HttpGet]
    public async Task<IActionResult> References(string? category, CancellationToken ct)
    {
        var all = await _db.ReferenceValues.AsNoTracking()
            .OrderBy(x => x.Category).ThenBy(x => x.SortOrder).ThenBy(x => x.Name)
            .Select(x => new AdminReferenceRowVm {
                Id=x.ReferenceValueId, Category=x.Category, Code=x.Code, Name=x.Name,
                Description=x.Description ?? "", SortOrder=x.SortOrder, IsActive=x.IsActive
            }).ToListAsync(ct);
        var selected = (category ?? "").Trim();
        return View(new AdminReferencesVm {
            Category=selected,
            Categories=all.Select(x=>x.Category).Distinct().ToList(),
            Rows=string.IsNullOrEmpty(selected) ? all : all.Where(x=>x.Category==selected).ToList()
        });
    }

    // Edit labels and sorting only; codes/categories/active flags remain immutable.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReference(AdminReferenceEditVm model, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!ModelState.IsValid) return BadRequest("Enter a valid reference label, description and sort order.");
        var row = await _db.ReferenceValues.SingleOrDefaultAsync(x=>x.ReferenceValueId==model.Id,ct);
        if (row is null) return NotFound();
        row.Name=model.Name.Trim(); row.Description=string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        row.SortOrder=model.SortOrder;
        GovernanceAudit.Add(_db,userId,"admin.reference-updated","ReferenceValue",model.Id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="Reference updated.";
        return RedirectToAction(nameof(References),new {category=row.Category});
    }

    [HttpGet]
    public async Task<IActionResult> Sectors(CancellationToken ct) => View(new AdminSectorsVm
    {
        Rows=await _db.Sectors.AsNoTracking().OrderBy(x=>x.Name)
            .Select(x=>new AdminSectorRowVm {Id=x.SectorId,Code=x.Code,Name=x.Name,Description=x.Description??""})
            .ToListAsync(ct)
    });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSector(AdminSectorEditVm model,CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!ModelState.IsValid) return BadRequest("Invalid sector details.");
        var row=await _db.Sectors.SingleOrDefaultAsync(x=>x.SectorId==model.Id,ct);
        if (row is null) return NotFound();
        row.Name=model.Name.Trim(); row.Description=string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        GovernanceAudit.Add(_db,userId,"admin.sector-updated","Sector",model.Id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="Sector updated.";
        return RedirectToAction(nameof(Sectors));
    }

    [HttpGet]
    public async Task<IActionResult> PrivacyNotices(CancellationToken ct) => View(new AdminNoticesVm
    {
        History=await _db.PrivacyNoticeVersions.AsNoTracking()
            .OrderByDescending(x=>x.PublishedAtUtc).Take(40)
            .Select(x=>new AdminNoticeRowVm {VersionLabel=x.VersionLabel, PublishedAtUtc= (DateTime)x.PublishedAtUtc })
            .ToListAsync(ct)
    });

    // Publish *only* privacy text reviewed by the project's responsible person.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishNotice(AdminPublishNoticeVm model,CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!ModelState.IsValid) return BadRequest("Provide a unique version and reviewed privacy notice of at least 200 characters.");
        var label=model.VersionLabel.Trim();
        if (await _db.PrivacyNoticeVersions.AnyAsync(x=>x.VersionLabel==label,ct))
            return BadRequest("That privacy notice version already exists.");
        var id=Guid.NewGuid();
        _db.PrivacyNoticeVersions.Add(new PrivacyNoticeVersion
        {
            PrivacyNoticeVersionId=id, VersionLabel=label, ContentStorageKey=null,
            ContentText=model.ContentText.Trim(), PublishedByUserId=userId,
            PublishedAtUtc=DateTime.UtcNow
        });
        GovernanceAudit.Add(_db,userId,"admin.privacy-notice-published","PrivacyNoticeVersion",id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="New privacy notice published. Verify the public page.";
        return RedirectToAction(nameof(PrivacyNotices));
    }

    [HttpGet]
    public async Task<IActionResult> RetentionPolicies(CancellationToken ct) => View(
        await _db.DataRetentionPolicies.AsNoTracking().OrderBy(x=>x.EntityType)
            .Select(x=>new AdminRetentionRowVm {
                EntityType=x.EntityType,RetentionDays=x.RetentionDays,
                Action=x.Action,IsActive=x.IsActive
            }).ToListAsync(ct));

    [HttpGet]
    public async Task<IActionResult> PrivacyRequests(CancellationToken ct)
    {
        var rows = await _db.DataSubjectRequests.AsNoTracking()
            .OrderBy(x => x.Status == "Completed")
            .ThenBy(x => x.DueAtUtc).Take(150)
            .Select(x => new AdminRequestRowVm
            {
                Id = x.DataSubjectRequestId,
                RequesterUserId = (Guid)x.RequesterUserId,
                AssignedToUserId = x.AssignedToUserId,
                RequestType = x.RequestType,
                Status = x.Status,
                ContactReferenceAvailable = x.VerifiedContactReference != null && x.VerifiedContactReference != "",
                ReceivedAtUtc = x.ReceivedAtUtc,
                DueAtUtc = (DateTime)x.DueAtUtc
            }).ToListAsync(ct);

        var ids = rows.Select(r => r.RequesterUserId)
            .Concat(rows.Where(r => r.AssignedToUserId.HasValue)
                        .Select(r => r.AssignedToUserId!.Value));
        var people = await AdminDisplayLookup.UsersAsync(_db, ids, ct);
        foreach (var row in rows)
        {
            if (people.TryGetValue(row.RequesterUserId, out var requester))
            {
                row.RequesterName = requester.Name;
                row.RequesterEmail = requester.Email;
            }
            else row.RequesterName = "Account unavailable";

            row.AssignedToName = row.AssignedToUserId is null ? "Unassigned"
                : people.TryGetValue(row.AssignedToUserId.Value, out var assignee)
                    ? assignee.Name : "Account unavailable";
        }
        return View(new AdminRequestsVm { Rows = rows });
    }

    // Merely triage/assign. This is not identity verification or request fulfilment.
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> StartReview(Guid id,CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var request=await _db.DataSubjectRequests.SingleOrDefaultAsync(x=>x.DataSubjectRequestId==id,ct);
        if (request is null) return NotFound();
        if (request.Status!="Received") return BadRequest("Only new requests can enter review.");
        request.Status="InProgress";
        request.AssignedToUserId=userId;
        GovernanceAudit.Add(_db,userId,"privacy.request-review-started","DataSubjectRequest",id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="Request assigned for review. Verify identity and follow your approved process before fulfilment.";
        return RedirectToAction(nameof(PrivacyRequests));
    }
}
