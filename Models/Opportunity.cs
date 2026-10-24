#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Opportunity
        {
            public Guid OpportunityId { get; set; }
            public Guid? OpportunitySourceId { get; set; }
            public Guid? SectorId { get; set; }
            public Guid? ProvinceId { get; set; }
            public Guid? MinimumStageId { get; set; }
            public Guid? MaximumStageId { get; set; }
            public string? ExternalId { get; set; }
            public string? ContentHash { get; set; }
            public int Revision { get; set; } = 1;
            public string OpportunityType { get; set; } = "";
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public string Status { get; set; } = "Active";
            public DateTime? OpensAtUtc { get; set; }
            public DateTime? ClosesAtUtc { get; set; }
            public OpportunitySource? Source { get; set; }
            public Sector? Sector { get; set; }
            public ICollection<OpportunityRequirement> Requirements { get; set; } = new List<OpportunityRequirement>();
            public ICollection<OpportunityMatch> Matches { get; set; } = new List<OpportunityMatch>();
            public ICollection<SavedOpportunity> SavedByBusinesses { get; set; } = new List<SavedOpportunity>();
        }
}
