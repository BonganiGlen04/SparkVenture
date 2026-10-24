#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class AuditLog
        {
            public Guid AuditLogId { get; set; }
            public Guid? ActorUserId { get; set; }
            public Guid? BusinessId { get; set; }
            public DateTime OccurredAtUtc { get; set; }
            public string ActionCode { get; set; } = "";
            public string EntityType { get; set; } = "";
            public string? EntityId { get; set; }
            public string? PayloadJson { get; set; }
            public ApplicationUser? ActorUser { get; set; }
            public Business? Business { get; set; }
        }
}
