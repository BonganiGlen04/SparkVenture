#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class FundingReadinessAssessment
        {
            public Guid FundingReadinessAssessmentId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? AssessedByUserId { get; set; }
            public string AssessmentVersion { get; set; } = "v1";
            public string Status { get; set; } = "Draft";
            public decimal? OverallScore { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? AssessedByUser { get; set; }
            public ICollection<FundingReadinessAnswer> Answers { get; set; } = new List<FundingReadinessAnswer>();
        }
}
