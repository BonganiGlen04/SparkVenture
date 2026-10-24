#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessInsight
        {
            public Guid BusinessInsightId { get; set; }
            public Guid BusinessId { get; set; }
            public BusinessInsightType InsightType { get; set; }
            public string Title { get; set; } = "";
            public string Explanation { get; set; } = "";
            public BusinessInsightSeverity Severity { get; set; }
            public DateTime GeneratedAtUtc { get; set; }
            public DateTime? DismissedAtUtc { get; set; }
            public string? RelatedEntityType { get; set; }
            public string? RelatedEntityId { get; set; }
            public DateTime? ExpiresAtUtc { get; set; }
            public Business? Business { get; set; }
        }
}
