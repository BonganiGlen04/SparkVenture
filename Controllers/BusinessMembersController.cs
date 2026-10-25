using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.Services;
using SparkVenture.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SparkVenture.Controllers;

// Integrates Step 1 Index with Step 2 Invite, Step 3 Accept, Step 4 Revoke.
[Authorize]
public class BusinessMembersController : Controller
{
    private static readonly string[] InviteRoles = { "Manager", "Accountant", "Advisor", "Observer" };
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBusinessInvitationEmailSender _emailSender;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BusinessMembersController> _logger;

    public BusinessMembersController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IBusinessInvitationEmailSender emailSender,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<BusinessMembersController> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    private bool TryCurrentUserId(out Guid userId)
        => Guid.TryParse(_userManager.GetUserId(User), out userId);

    private Task<bool> IsOwnerAsync(Guid businessId, Guid userId, CancellationToken ct)
        => _db.BusinessMembers.AnyAsync(m =>
            m.BusinessId == businessId && m.UserId == userId &&
            m.MembershipStatus == "Active" && m.BusinessRole == "Owner", ct);

    // STEP 1 — keep previously working read-only Index, now with invitation link + revoke controls.
    [HttpGet]
    public async Task<IActionResult> Index(Guid businessId, CancellationToken cancellationToken)
    {
        if (!TryCurrentUserId(out var userId)) return Challenge();
        if (businessId == Guid.Empty) return RedirectToAction("Index", "Businesses");

        var myRole = await _db.BusinessMembers.AsNoTracking()
            .Where(m => m.BusinessId == businessId && m.UserId == userId
                && m.MembershipStatus == "Active")
            .Select(m => m.BusinessRole)
            .FirstOrDefaultAsync(cancellationToken);
        if (myRole == null) return NotFound();

        var businessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == businessId)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);
        if (businessName == null) return NotFound();

        var members = await (
            from member in _db.BusinessMembers.AsNoTracking()
            join user in _db.Users.AsNoTracking() on member.UserId equals user.Id
            where member.BusinessId == businessId &&
                (member.MembershipStatus == "Active" || member.MembershipStatus == "Pending")
            orderby member.BusinessRole == "Owner" descending, member.BusinessRole, user.Email
            select new BusinessMemberRowViewModel
            {
                BusinessMemberId = member.BusinessMemberId,
                UserId = member.UserId,
                DisplayName = user.UserName ?? user.Email ?? "Team member",
                Email = user.Email ?? "",
                Role = member.BusinessRole,
                Status = member.MembershipStatus,
                IsCurrentUser = member.UserId == userId
            }).ToListAsync(cancellationToken);

        var canManage = myRole == "Owner";
        var invitations = new List<BusinessInvitationRowViewModel>();
        if (canManage)
        {
            var utcNow = DateTime.UtcNow;
            invitations = await _db.BusinessInvitations.AsNoTracking()
                .Where(i => i.BusinessId == businessId &&
                    i.AcceptedAtUtc == null && i.ExpiresAtUtc > utcNow)
                .OrderBy(i => i.ExpiresAtUtc)
                .Select(i => new BusinessInvitationRowViewModel
                {
                    BusinessInvitationId = i.BusinessInvitationId,
                    Email = i.Email,
                    Role = i.BusinessRole,
                    ExpiresAtUtc = i.ExpiresAtUtc
                }).ToListAsync(cancellationToken);
        }

        if (_environment.IsDevelopment())
            ViewData["DevelopmentInviteUrl"] = TempData["DevelopmentInviteUrl"] as string;

        return View(new BusinessMembersIndexViewModel
        {
            BusinessId = businessId,
            BusinessName = businessName,
            CurrentUserRole = myRole,
            CanManageMembers = canManage,
            Members = members,
            PendingInvitations = invitations
        });
    }

    // STEP 2 — only the business Owner may invite; Owner itself isn't assignable.
    [HttpGet]
    public async Task<IActionResult> Invite(Guid businessId, CancellationToken cancellationToken)
    {
        if (!TryCurrentUserId(out var userId)) return Challenge();
        if (!await IsOwnerAsync(businessId, userId, cancellationToken)) return NotFound();

        var name = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == businessId)
            .Select(b => b.TradingName).FirstOrDefaultAsync(cancellationToken);
        if (name == null) return NotFound();

        return View(new InviteBusinessMemberViewModel { BusinessId = businessId, BusinessName = name });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteBusinessMemberViewModel model, CancellationToken cancellationToken)
    {
        if (!TryCurrentUserId(out var ownerId)) return Challenge();
        if (!await IsOwnerAsync(model.BusinessId, ownerId, cancellationToken)) return NotFound();

        model.BusinessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == model.BusinessId)
            .Select(b => b.TradingName).FirstOrDefaultAsync(cancellationToken) ?? "";
        if (model.BusinessName.Length == 0) return NotFound();

        model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
        model.Role = (model.Role ?? "").Trim();
        if (!InviteRoles.Contains(model.Role, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.Role), "Please choose an allowed role.");
        // The email was normalized after model binding; revalidate explicitly.
        if (model.Email.Length == 0 || model.Email.Contains('\r') || model.Email.Contains('\n'))
            ModelState.AddModelError(nameof(model.Email), "Please enter a valid email address.");

        if (!ModelState.IsValid) return View(model);

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            var active = await _db.BusinessMembers.AnyAsync(m =>
                m.BusinessId == model.BusinessId && m.UserId == existingUser.Id
                && m.MembershipStatus == "Active", cancellationToken);
            if (active)
            {
                ModelState.AddModelError(nameof(model.Email), "This person is already an active member.");
                return View(model);
            }
        }

        // Serialize concurrent invitations for the same business/address.
        // Add an index on (BusinessId, Email) in the real EF model for efficient locking.
        await using var createTx = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        var now = DateTime.UtcNow;
        var normalisedEmail = model.Email.ToUpperInvariant();
        var pending = await _db.BusinessInvitations.AnyAsync(i =>
            i.BusinessId == model.BusinessId && i.AcceptedAtUtc == null &&
            i.ExpiresAtUtc > now && i.Email.ToUpper() == normalisedEmail, cancellationToken);
        if (pending)
        {
            ModelState.AddModelError(nameof(model.Email), "An active invitation already exists for this address.");
            return View(model);
        }

        // Production must have a configured sender + canonical HTTPS base URL.
        if (!_environment.IsDevelopment() && !_emailSender.IsConfigured)
        {
            ModelState.AddModelError("", "Invitation email delivery is not configured. Ask an administrator to set it up.");
            return View(model);
        }

        var relativePath = Url.Action("Join", "BusinessMembers", new { token = "PLACEHOLDER" });
        if (relativePath == null)
        {
            ModelState.AddModelError("", "Invitation route could not be generated.");
            return View(model);
        }

        var baseUrl = _configuration["InvitationLinks:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl) && _environment.IsDevelopment())
            baseUrl = $"{Request.Scheme}://{Request.Host}";

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var siteUri) ||
            (!_environment.IsDevelopment() && siteUri.Scheme != Uri.UriSchemeHttps))
        {
            ModelState.AddModelError("", "Configure InvitationLinks:BaseUrl with your site's HTTPS address.");
            return View(model);
        }

        // 256-bit single-use token. Database stores ONLY the SHA-256 hash.
        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var tokenHash = HashToken(rawToken);
        var invitePath = Url.Action("Join", "BusinessMembers", new { token = rawToken })!;
        var link = new Uri(new Uri(siteUri.ToString().TrimEnd('/') + "/"), invitePath.TrimStart('/')).ToString();

        var invite = new BusinessInvitation
        {
            BusinessInvitationId = Guid.NewGuid(),
            BusinessId = model.BusinessId,
            InvitedByUserId = ownerId,
            Email = model.Email,
            BusinessRole = model.Role,
            TokenHash = tokenHash,
            ExpiresAtUtc = now.Add(InviteLifetime),
            AcceptedAtUtc = null
        };
        _db.BusinessInvitations.Add(invite);
        AddAudit(ownerId, model.BusinessId, "business.invitation-created", "BusinessInvitation",
            invite.BusinessInvitationId, new { role = model.Role });
        await _db.SaveChangesAsync(cancellationToken);
        await createTx.CommitAsync(cancellationToken);

        if (_environment.IsDevelopment() && !_emailSender.IsConfigured)
        {
            // Display ONCE, to the inviting Owner only. Do not log the token/link.
            TempData["DevelopmentInviteUrl"] = link;
            TempData["SuccessMessage"] = "Invitation created. Copy the development link and share it privately with the invitee.";
        }
        else
        {
            try
            {
                await _emailSender.SendAsync(model.Email, model.BusinessName, model.Role, link, cancellationToken);
                TempData["SuccessMessage"] = "Invitation sent by email.";
            }
            catch (Exception ex)
            {
                // Stop the failed email invitation being used. Owner may invite again.
                invite.ExpiresAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(CancellationToken.None);
                _logger.LogError(ex, "Invitation delivery failed for business {BusinessId}, invitation {InvitationId}",
                    model.BusinessId, invite.BusinessInvitationId);
                TempData["ErrorMessage"] = "Email could not be sent. No active invitation remains; you can try again.";
            }
        }

        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }

    // Optional but valuable: withdraw an invitation before it is accepted.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelInvitation(
        Guid businessId, Guid invitationId, CancellationToken cancellationToken)
    {
        if (!TryCurrentUserId(out var ownerId)) return Challenge();
        if (!await IsOwnerAsync(businessId, ownerId, cancellationToken)) return NotFound();

        var invite = await _db.BusinessInvitations.FirstOrDefaultAsync(i =>
            i.BusinessInvitationId == invitationId && i.BusinessId == businessId,
            cancellationToken);
        if (invite == null) return NotFound();
        if (invite.AcceptedAtUtc != null || invite.ExpiresAtUtc <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "The invitation was already used or has expired.";
            return RedirectToAction(nameof(Index), new { businessId });
        }

        invite.ExpiresAtUtc = DateTime.UtcNow;
        AddAudit(ownerId, businessId, "business.invitation-cancelled", "BusinessInvitation",
            invite.BusinessInvitationId, new { role = invite.BusinessRole });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = "Invitation cancelled.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    // STEP 3 — public, neutral landing page: it reveals no business details.
    [AllowAnonymous, HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Join(string token)
    {
        if (!LooksLikeToken(token)) return InvitationProblem("Invalid invitation", "This invitation link is invalid.");

        var acceptPath = Url.Action(nameof(Accept), "BusinessMembers", new { token })!;
        var loginUrl = Url.Page("/Account/Login", new { area = "Identity", ReturnUrl = acceptPath }) ?? "/Identity/Account/Login";
        var registerUrl = Url.Page("/Account/Register", new { area = "Identity", ReturnUrl = acceptPath }) ?? "/Identity/Account/Register";
        return View(new JoinBusinessInvitationViewModel
        {
            LoginUrl = loginUrl,
            RegisterUrl = registerUrl
        });
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Accept(string token, CancellationToken cancellationToken)
    {
        if (!LooksLikeToken(token)) return InvitationProblem("Invalid invitation", "This invitation link is invalid.");
        var invitation = await GetOpenInvitationAsync(token, cancellationToken);
        if (invitation == null)
            return InvitationProblem("Invitation unavailable", "This invitation was used, expired or revoked. Ask the business owner for a new one.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        if (!await IsVerifiedRecipientAsync(user, invitation.Email))
            return InvitationProblem("Confirm your email address", "Sign in with the invited email address and verify it before accepting.");

        var businessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == invitation.BusinessId)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);
        if (businessName == null) return InvitationProblem("Invitation unavailable", "This business is no longer available.");

        return View(new AcceptBusinessInvitationViewModel
        {
            Token = token,
            BusinessName = businessName,
            InvitedEmail = invitation.Email,
            Role = invitation.BusinessRole
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptInvitation(string token, CancellationToken cancellationToken)
    {
        if (!LooksLikeToken(token)) return InvitationProblem("Invalid invitation", "This invitation link is invalid.");
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Serializable transaction protects the invitation state + membership + audit as one unit.
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var invitation = await GetOpenInvitationAsync(token, cancellationToken);
        if (invitation == null)
            return InvitationProblem("Invitation unavailable", "This invitation was already used or has expired.");

        if (!await IsVerifiedRecipientAsync(user, invitation.Email))
            return InvitationProblem("Confirm your email address", "Use the invited email account and confirm your address first.");

        var member = await _db.BusinessMembers.FirstOrDefaultAsync(m =>
            m.BusinessId == invitation.BusinessId && m.UserId == user.Id, cancellationToken);

        if (member?.MembershipStatus == "Active")
            return InvitationProblem("Already a member", "You already belong to this business. Ask the owner if you need a different role.");

        if (member == null)
        {
            member = new BusinessMember
            {
                BusinessMemberId = Guid.NewGuid(),
                BusinessId = invitation.BusinessId,
                UserId = user.Id,
                InvitedByUserId = invitation.InvitedByUserId,
                InvitedAtUtc = DateTime.UtcNow,
                BusinessRole = invitation.BusinessRole,
                MembershipStatus = "Active"
            };
            _db.BusinessMembers.Add(member);
        }
        else
        {
            // Re-invitation may activate a Pending or previously Revoked membership.
            member.MembershipStatus = "Active";
            member.BusinessRole = invitation.BusinessRole;
            member.InvitedByUserId = invitation.InvitedByUserId;
            member.InvitedAtUtc = DateTime.UtcNow;
        }

        invitation.AcceptedAtUtc = DateTime.UtcNow;
        AddAudit(user.Id, invitation.BusinessId, "business.invitation-accepted", "BusinessMember",
            member.BusinessMemberId, new { role = member.BusinessRole });

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        TempData["SuccessMessage"] = "Invitation accepted. You now have access to the business.";
        return RedirectToAction("Index", "Dashboard", new { businessId = invitation.BusinessId });
    }

    // STEP 4 — POST only, owner only, never delete rows; Owner cannot be revoked here.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(Guid businessId, Guid memberId, CancellationToken cancellationToken)
    {
        if (!TryCurrentUserId(out var ownerId)) return Challenge();

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        if (!await IsOwnerAsync(businessId, ownerId, cancellationToken)) return NotFound();

        var member = await _db.BusinessMembers.FirstOrDefaultAsync(m =>
            m.BusinessMemberId == memberId && m.BusinessId == businessId, cancellationToken);
        if (member == null) return NotFound();
        if (member.UserId == ownerId || member.BusinessRole == "Owner")
        {
            TempData["ErrorMessage"] = "Owner access cannot be revoked here.";
            return RedirectToAction(nameof(Index), new { businessId });
        }
        if (member.MembershipStatus != "Active" && member.MembershipStatus != "Pending")
        {
            TempData["ErrorMessage"] = "This membership is not active or pending.";
            return RedirectToAction(nameof(Index), new { businessId });
        }

        var revokedRole = member.BusinessRole;
        member.MembershipStatus = "Revoked";

        // Also close any live, unaccepted invitations for this address/business,
        // so an old link cannot immediately reactivate a revoked membership.
        var email = await _db.Users.AsNoTracking()
            .Where(u => u.Id == member.UserId).Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(email))
        {
            var now = DateTime.UtcNow;
            var normalized = email.Trim().ToUpperInvariant();
            var openInvites = await _db.BusinessInvitations
                .Where(i => i.BusinessId == businessId && i.AcceptedAtUtc == null &&
                    i.ExpiresAtUtc > now && i.Email.ToUpper() == normalized)
                .ToListAsync(cancellationToken);
            foreach (var invite in openInvites) invite.ExpiresAtUtc = now;
        }

        AddAudit(ownerId, businessId, "business.member-revoked", "BusinessMember",
            member.BusinessMemberId, new { targetUserId = member.UserId, role = revokedRole });
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        TempData["SuccessMessage"] = "Member access revoked. Historical records have been preserved.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    private static bool LooksLikeToken(string? token)
        => !string.IsNullOrWhiteSpace(token) && token.Length >= 40 && token.Length <= 90 &&
            token.All(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_');

    private static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private Task<BusinessInvitation?> GetOpenInvitationAsync(string token, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);
        var now = DateTime.UtcNow;
        return _db.BusinessInvitations.FirstOrDefaultAsync(i =>
            i.TokenHash == hash && i.AcceptedAtUtc == null && i.ExpiresAtUtc > now, cancellationToken);
    }

    private async Task<bool> IsVerifiedRecipientAsync(ApplicationUser user, string invitedEmail)
    {
        var currentEmail = await _userManager.GetEmailAsync(user);
        return !string.IsNullOrWhiteSpace(currentEmail) &&
            string.Equals(currentEmail.Trim(), invitedEmail.Trim(), StringComparison.OrdinalIgnoreCase) &&
            await _userManager.IsEmailConfirmedAsync(user);
    }

    private IActionResult InvitationProblem(string heading, string message)
        => View("InvitationMessage", new InvitationMessageViewModel { Heading = heading, Message = message });

    private void AddAudit(Guid actorId, Guid businessId, string action, string entityType,
        Guid entityId, object details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            ActorUserId = actorId,
            BusinessId = businessId,
            OccurredAtUtc = DateTime.UtcNow,
            ActionCode = action,
            EntityType = entityType,
            EntityId = entityId.ToString(),
            PayloadJson = JsonSerializer.Serialize(details)
        });
    }
}
