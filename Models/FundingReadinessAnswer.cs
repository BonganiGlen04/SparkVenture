#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class FundingReadinessAnswer
        {
            public Guid FundingReadinessAnswerId { get; set; }
            public Guid FundingReadinessAssessmentId { get; set; }
            public Guid? EvidenceDocumentId { get; set; }
            public string QuestionKey { get; set; } = "";
            public string AnswerValue { get; set; } = "";
            public decimal? Score { get; set; }
            public FundingReadinessAssessment? Assessment { get; set; }
            public Document? EvidenceDocument { get; set; }
        }
}
