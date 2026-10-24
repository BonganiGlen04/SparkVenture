#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RecommendationFeedback
        {
            public Guid RecommendationFeedbackId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid OpportunityId { get; set; }
            public Guid UserId { get; set; }
            public Guid? OpportunityMatchId { get; set; }
            public Guid? TenderApplicationId { get; set; }
            public Guid? FundingApplicationId { get; set; }
            public string FeedbackType { get; set; } = "";
            public string? ReasonCode { get; set; }
            public string? Comment { get; set; }
            public DateTime RecordedAtUtc { get; set; }
            public Business? Business { get; set; }
            public Opportunity? Opportunity { get; set; }
            public ApplicationUser? User { get; set; }
            public OpportunityMatch? OpportunityMatch { get; set; }
        }
}
