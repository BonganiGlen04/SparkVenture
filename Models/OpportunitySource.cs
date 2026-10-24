#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class OpportunitySource
        {
            public Guid OpportunitySourceId { get; set; }
            public string Name { get; set; } = "";
            public string SourceType { get; set; } = "";
            public string? WebsiteUrl { get; set; }
            public bool IsActive { get; set; } = true;
            public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        }
}
