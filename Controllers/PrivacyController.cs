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

[Authorize]
public sealed class PrivacyController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public PrivacyController(ApplicationDbContext db,UserManager<ApplicationUser> users)
    { _db=db; _users=users; }
    private bool CurrentUser(out Guid id)=>Guid.TryParse(_users.GetUserId(User),out id);

    private async Task<PrivacyNoticeVm> GetNoticeAsync(CancellationToken ct)
    {
        var notice=await _db.PrivacyNoticeVersions.AsNoTracking()
            .OrderByDescending(x=>x.PublishedAtUtc).Select(x=>new PrivacyNoticeVm {
                VersionLabel=x.VersionLabel,PublishedAtUtc=x.PublishedAtUtc,Text=x.ContentText??""
            }).FirstOrDefaultAsync(ct);
        if (notice is null) return new PrivacyNoticeVm {IsDemoOrMissing=true,Text="No privacy notice has been published yet."};
        notice.IsDemoOrMissing=string.IsNullOrWhiteSpace(notice.Text) ||
            notice.Text.TrimStart().StartsWith("Dummy",StringComparison.OrdinalIgnoreCase);
        return notice;
    }

    [AllowAnonymous,HttpGet]
    public async Task<IActionResult> Notice(CancellationToken ct) => View(await GetNoticeAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var lastConsent=await _db.ConsentEvents.AsNoTracking()
            .Where(x=>x.UserId==userId && x.PurposeCode=="marketing-emails")
            .OrderByDescending(x=>x.OccurredAtUtc)
            .Select(x=>x.EventType).FirstOrDefaultAsync(ct);
        var requests=await _db.DataSubjectRequests.AsNoTracking()
            .Where(x=>x.RequesterUserId==userId).OrderByDescending(x=>x.ReceivedAtUtc).Take(30)
            .Select(x=>new PrivacyRequestRowVm {
                RequestType=x.RequestType,
                Status=x.Status,
                ReceivedAtUtc=x.ReceivedAtUtc,
                DueAtUtc=x.DueAtUtc ?? DateTime.MinValue
            }).ToListAsync(ct);
        return View(new PrivacyIndexVm {
            Notice=await GetNoticeAsync(ct),
            MarketingChoice=lastConsent=="Granted"?true:lastConsent=="Withdrawn"?false:(bool?)null,
            Requests=requests
        });
    }

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMarketingChoice(bool allowMarketing,CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var notice=await _db.PrivacyNoticeVersions.AsNoTracking().OrderByDescending(x=>x.PublishedAtUtc).FirstOrDefaultAsync(ct);
        var hasReviewedNotice=notice is not null && !string.IsNullOrWhiteSpace(notice.ContentText) &&
            !notice.ContentText.TrimStart().StartsWith("Dummy",StringComparison.OrdinalIgnoreCase);
        // An existing marketing opt-in may be withdrawn even while only demo notice text exists.
        // New opt-ins, however, require genuine reviewed notice content.
        if (allowMarketing && !hasReviewedNotice)
            return BadRequest("Publish a genuine reviewed privacy notice before accepting new marketing opt-ins.");
        var desired=allowMarketing?"Granted":"Withdrawn";
        var previous=await _db.ConsentEvents.AsNoTracking()
            .Where(x=>x.UserId==userId && x.PurposeCode=="marketing-emails")
            .OrderByDescending(x=>x.OccurredAtUtc).FirstOrDefaultAsync(ct);
        if (previous?.EventType==desired) return RedirectToAction(nameof(Index));
        if (!allowMarketing && previous is null)
        {
            TempData["SuccessMessage"]="No marketing opt-in is recorded for your account.";
            return RedirectToAction(nameof(Index));
        }
        var id=Guid.NewGuid();
        _db.ConsentEvents.Add(new ConsentEvent {
            ConsentEventId=id,UserId=userId,BusinessId=null,ActorUserId=userId,
            PrivacyNoticeVersionId=allowMarketing ? notice!.PrivacyNoticeVersionId : previous!.PrivacyNoticeVersionId,
            PurposeCode="marketing-emails",EventType=desired,
            ConsentTextVersion=allowMarketing ? notice!.VersionLabel : previous!.ConsentTextVersion,
            OccurredAtUtc=DateTime.UtcNow
        });
        GovernanceAudit.Add(_db,userId,"privacy.marketing-consent-changed","ConsentEvent",id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="Your marketing-email choice has been recorded.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRequest(PrivacyRequestInputVm model,CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!ModelState.IsValid) return BadRequest("Choose Access, Correction or Deletion.");
        var exists=await _db.DataSubjectRequests.AsNoTracking().AnyAsync(x=>
            x.RequesterUserId==userId && x.RequestType==model.RequestType &&
            (x.Status=="Received" || x.Status=="InProgress"),ct);
        if (exists)
        {
            TempData["SuccessMessage"]="An open request of this type already exists. You can follow its status below.";
            return RedirectToAction(nameof(Index));
        }
        var id=Guid.NewGuid();
        var now=DateTime.UtcNow;
        _db.DataSubjectRequests.Add(new DataSubjectRequest {
            DataSubjectRequestId=id,RequesterUserId=userId,
            AssignedToUserId=null,
            // No claim of verification. Admin must verify identity through an approved process.
            VerifiedContactReference="",
            RequestType=model.RequestType,Status="Received",
            ReceivedAtUtc=now,DueAtUtc=now.AddDays(30),CompletedAtUtc=null
        });
        GovernanceAudit.Add(_db,userId,"privacy.request-received","DataSubjectRequest",id);
        await _db.SaveChangesAsync(ct);
        TempData["SuccessMessage"]="Your request was recorded. The displayed date is an internal follow-up target, not a promise of automated fulfilment.";
        return RedirectToAction(nameof(Index));
    }
}
