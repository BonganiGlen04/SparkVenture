#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class FundingApplicationDocument
        {
            public Guid FundingApplicationDocumentId { get; set; }
            public Guid FundingApplicationId { get; set; }
            public Guid DocumentId { get; set; }
            public Guid BusinessId { get; set; }
            public string DocumentPurpose { get; set; } = "";
            public bool IsRequiredEvidence { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public FundingApplication? FundingApplication { get; set; }
            public Document? Document { get; set; }
            public Business? Business { get; set; }
        }
}
