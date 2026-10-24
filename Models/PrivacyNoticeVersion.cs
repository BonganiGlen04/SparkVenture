#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class PrivacyNoticeVersion
        {
            public Guid PrivacyNoticeVersionId { get; set; }
            public string VersionLabel { get; set; } = "";
            public string? ContentStorageKey { get; set; }
            public string? ContentText { get; set; }
            public Guid? PublishedByUserId { get; set; }
            public DateTime? PublishedAtUtc { get; set; }
            public ApplicationUser? PublishedByUser { get; set; }
        }
}
