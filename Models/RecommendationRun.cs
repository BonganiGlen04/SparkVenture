#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RecommendationRun
        {
            public Guid RecommendationRunId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? TriggeredByUserId { get; set; }
            public string RunType { get; set; } = "";
            public string AlgorithmVersion { get; set; } = "v1";
            public bool IsCurrent { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? TriggeredByUser { get; set; }
            public ICollection<OpportunityMatch> Matches { get; set; } = new List<OpportunityMatch>();
        }
}
