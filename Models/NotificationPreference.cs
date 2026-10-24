#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class NotificationPreference
        {
            public Guid NotificationPreferenceId { get; set; }
            public Guid UserId { get; set; }
            public string Channel { get; set; } = "";
            public string Category { get; set; } = "";
            public bool IsEnabled { get; set; } = true;
            public DateTime UpdatedAtUtc { get; set; }
            public ApplicationUser? User { get; set; }
        }
}
