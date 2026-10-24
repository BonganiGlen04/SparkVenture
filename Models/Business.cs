#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Business
        {
            public Guid BusinessId { get; set; }
            public Guid BusinessTypeId { get; set; }
            public Guid BusinessStageId { get; set; }
            public Guid? SectorId { get; set; }
            public Guid? ProvinceId { get; set; }
            public Guid? EmployeeCountBandId { get; set; }
            public Guid? AnnualRevenueBandId { get; set; }
            public Guid CreatedByUserId { get; set; }
            public string? LegalName { get; set; }
            public string TradingName { get; set; } = "";
            public string? RegistrationNumber { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public byte[]? RowVersion { get; set; }
    
            public Sector? Sector { get; set; }
            public ApplicationUser? CreatedByUser { get; set; }
            public ICollection<BusinessMember> Members { get; set; } = new List<BusinessMember>();
            public ICollection<BusinessAddress> Addresses { get; set; } = new List<BusinessAddress>();
            public ICollection<BusinessCredential> Credentials { get; set; } = new List<BusinessCredential>();
            public ICollection<BusinessInvitation> Invitations { get; set; } = new List<BusinessInvitation>();
            public ICollection<BusinessMarketLink> MarketLinks { get; set; } = new List<BusinessMarketLink>();
            public ICollection<BusinessInsight> Insights { get; set; } = new List<BusinessInsight>();
            public ICollection<BusinessSignal> Signals { get; set; } = new List<BusinessSignal>();
            public ICollection<Customer> Customers { get; set; } = new List<Customer>();
            public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
            public ICollection<CashbookEntry> CashbookEntries { get; set; } = new List<CashbookEntry>();
            public ICollection<OpportunityMatch> OpportunityMatches { get; set; } = new List<OpportunityMatch>();
            public ICollection<SavedOpportunity> SavedOpportunities { get; set; } = new List<SavedOpportunity>();
            public ICollection<RecommendationRun> RecommendationRuns { get; set; } = new List<RecommendationRun>();
            public ICollection<RecommendationFeedback> RecommendationFeedback { get; set; } = new List<RecommendationFeedback>();
            public ICollection<RegistrationApplication> RegistrationApplications { get; set; } = new List<RegistrationApplication>();
            public ICollection<TenderApplication> TenderApplications { get; set; } = new List<TenderApplication>();
            public ICollection<FundingApplication> FundingApplications { get; set; } = new List<FundingApplication>();
        }
}
