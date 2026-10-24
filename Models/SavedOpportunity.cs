#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class SavedOpportunity
        {
            public Guid SavedOpportunityId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid OpportunityId { get; set; }
            public Guid SavedByUserId { get; set; }
            public DateTime SavedAtUtc { get; set; }
            public string? Notes { get; set; }
            public Business? Business { get; set; }
            public Opportunity? Opportunity { get; set; }
            public ApplicationUser? SavedByUser { get; set; }
        }
}
