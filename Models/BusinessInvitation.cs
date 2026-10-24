#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessInvitation
        {
            public Guid BusinessInvitationId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? InvitedByUserId { get; set; }
            public string Email { get; set; } = "";
            public string BusinessRole { get; set; } = "Viewer";
            public string TokenHash { get; set; } = "";
            public DateTime ExpiresAtUtc { get; set; }
            public DateTime? AcceptedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? InvitedByUser { get; set; }
        }
}
