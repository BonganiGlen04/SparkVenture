#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class OpportunityMatch
        {
            public Guid OpportunityMatchId { get; set; }
            public Guid RecommendationRunId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid OpportunityId { get; set; }
            public int OpportunityRevision { get; set; }
            public decimal Score { get; set; }
            public string MatchBand { get; set; } = "";
            public bool MandatoryCriteriaPassed { get; set; }
            public string Explanation { get; set; } = "";
            public string? BoostExplanation { get; set; }
            public string AlgorithmVersion { get; set; } = "v1";
            public Business? Business { get; set; }
            public RecommendationRun? RecommendationRun { get; set; }
            public Opportunity? Opportunity { get; set; }
        }
}
