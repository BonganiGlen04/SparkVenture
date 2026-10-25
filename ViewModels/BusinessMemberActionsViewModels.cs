using System;
using System.ComponentModel.DataAnnotations;

namespace SparkVenture.ViewModels;

public sealed class InviteBusinessMemberViewModel
{
    [Required]
    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    [Display(Name = "Invitation email address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Team member role")]
    public string Role { get; set; } = "Observer";
}

public sealed class JoinBusinessInvitationViewModel
{
    public string LoginUrl { get; set; } = string.Empty;
    public string RegisterUrl { get; set; } = string.Empty;
}

public sealed class AcceptBusinessInvitationViewModel
{
    public string Token { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string InvitedEmail { get; set; } = string.Empty;
}

public sealed class InvitationMessageViewModel
{
    public string Heading { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
