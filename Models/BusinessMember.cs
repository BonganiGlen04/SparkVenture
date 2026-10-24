#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessMember
        {
            public Guid BusinessMemberId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid UserId { get; set; }
            public Guid? InvitedByUserId { get; set; }
            public string BusinessRole { get; set; } = "Viewer";
            public string MembershipStatus { get; set; } = "Active";
            public DateTime InvitedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? User { get; set; }
            public ApplicationUser? InvitedByUser { get; set; }
        }
}
