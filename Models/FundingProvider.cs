#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class FundingProvider
        {
            public Guid FundingProviderId { get; set; }
            public string Name { get; set; } = "";
            public string ProviderType { get; set; } = "";
            public string? WebsiteUrl { get; set; }
            public bool IsActive { get; set; } = true;
            public ICollection<FundingApplication> Applications { get; set; } = new List<FundingApplication>();
        }
}
