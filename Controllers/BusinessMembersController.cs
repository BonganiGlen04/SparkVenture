using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

// STEP 1: Read-only team roster. Invite/Accept/Revoke are separate steps.
[Authorize]
public class BusinessMembersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusinessMembersController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET /BusinessMembers?businessId=<guid>
    [HttpGet]
    public async Task<IActionResult> Index(Guid businessId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_userManager.GetUserId(User), out var userId))
            return Challenge();

        if (businessId == Guid.Empty)
            return RedirectToAction("Index", "Businesses");

        // Check membership BEFORE reading the business's members or invitations.
        var myRole = await _db.BusinessMembers
            .AsNoTracking()
            .Where(m => m.BusinessId == businessId
                        && m.UserId == userId
                        && m.MembershipStatus == "Active")
            .Select(m => m.BusinessRole)
            .FirstOrDefaultAsync(cancellationToken);

        // 404 prevents revealing whether a business exists to other users.
        if (myRole == null)
            return NotFound();

        var businessName = await _db.Businesses
            .AsNoTracking()
            .Where(b => b.BusinessId == businessId)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);

        if (businessName == null)
            return NotFound();

        // The seeded schema includes both Active and Pending memberships.
        // Don't show revoked/removed members in the ordinary roster.
        var members = await (
            from member in _db.BusinessMembers.AsNoTracking()
            join user in _db.Users.AsNoTracking() on member.UserId equals user.Id
            where member.BusinessId == businessId
                  && (member.MembershipStatus == "Active" || member.MembershipStatus == "Pending")
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

        // An owner can inspect outstanding invitations. Ordinary members cannot.
        var canManageMembers = myRole == "Owner";
        var pendingInvitations = new System.Collections.Generic.List<BusinessInvitationRowViewModel>();

        if (canManageMembers)
        {
            var utcNow = DateTime.UtcNow;
            pendingInvitations = await _db.BusinessInvitations
                .AsNoTracking()
                .Where(i => i.BusinessId == businessId
                            && i.AcceptedAtUtc == null
                            && i.ExpiresAtUtc > utcNow)
                .OrderBy(i => i.ExpiresAtUtc)
                .Select(i => new BusinessInvitationRowViewModel
                {
                    BusinessInvitationId = i.BusinessInvitationId,
                    Email = i.Email,
                    Role = i.BusinessRole,
                    ExpiresAtUtc = i.ExpiresAtUtc
                })
                .ToListAsync(cancellationToken);
        }

        return View(new BusinessMembersIndexViewModel
        {
            BusinessId = businessId,
            BusinessName = businessName,
            CurrentUserRole = myRole,
            CanManageMembers = canManageMembers,
            Members = members,
            PendingInvitations = pendingInvitations
        });
    }
}
