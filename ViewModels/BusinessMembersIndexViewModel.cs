using System;
using System.Collections.Generic;
using System.Linq;

namespace SparkVenture.ViewModels;

public sealed class BusinessMembersIndexViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string CurrentUserRole { get; set; } = string.Empty;
    public bool CanManageMembers { get; set; }

    public List<BusinessMemberRowViewModel> Members { get; set; } = new();
    public List<BusinessInvitationRowViewModel> PendingInvitations { get; set; } = new();

    public int ActiveCount => Members.Count(m => m.Status == "Active");
    public int PendingMembershipCount => Members.Count(m => m.Status == "Pending");
}

public sealed class BusinessMemberRowViewModel
{
    public Guid BusinessMemberId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsCurrentUser { get; set; }
}

public sealed class BusinessInvitationRowViewModel
{
    public Guid BusinessInvitationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
