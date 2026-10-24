#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class FundingApplication
        {
            public Guid FundingApplicationId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? OpportunityId { get; set; }
            public Guid? FundingProviderId { get; set; }
            public Guid CreatedByUserId { get; set; }
            public string Status { get; set; } = "Draft";
            public decimal? RequestedAmount { get; set; }
            public string? CurrencyCode { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public Opportunity? Opportunity { get; set; }
            public FundingProvider? FundingProvider { get; set; }
            public ApplicationUser? CreatedByUser { get; set; }
            public ICollection<FundingApplicationDocument> Documents { get; set; } = new List<FundingApplicationDocument>();
        }
}
