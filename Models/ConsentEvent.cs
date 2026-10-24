#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class ConsentEvent
        {
            public Guid ConsentEventId { get; set; }
            public Guid UserId { get; set; }
            public Guid? BusinessId { get; set; }
            public Guid? ActorUserId { get; set; }
            public Guid? PrivacyNoticeVersionId { get; set; }
            public string PurposeCode { get; set; } = "";
            public string EventType { get; set; } = "";
            public string? ConsentTextVersion { get; set; }
            public DateTime OccurredAtUtc { get; set; }
            public ApplicationUser? User { get; set; }
            public ApplicationUser? ActorUser { get; set; }
            public Business? Business { get; set; }
            public PrivacyNoticeVersion? PrivacyNoticeVersion { get; set; }
        }
}
