using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data.Models;

namespace SparkVenture.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
        public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Sector> Sectors => Set<Sector>();
        public DbSet<ReferenceValue> ReferenceValues => Set<ReferenceValue>();
        public DbSet<Business> Businesses => Set<Business>();
        public DbSet<BusinessMember> BusinessMembers => Set<BusinessMember>();
        public DbSet<BusinessAddress> BusinessAddresses => Set<BusinessAddress>();
        public DbSet<BusinessCredential> BusinessCredentials => Set<BusinessCredential>();
        public DbSet<BusinessInvitation> BusinessInvitations => Set<BusinessInvitation>();
        public DbSet<BusinessMarketLink> BusinessMarketLinks => Set<BusinessMarketLink>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<BusinessDocument> BusinessDocuments => Set<BusinessDocument>();
        public DbSet<RegistrationApplicationDocument> RegistrationApplicationDocuments => Set<RegistrationApplicationDocument>();
        public DbSet<InvoiceDocument> InvoiceDocuments => Set<InvoiceDocument>();
        public DbSet<CashbookEntryDocument> CashbookEntryDocuments => Set<CashbookEntryDocument>();
        public DbSet<TenderApplicationDocument> TenderApplicationDocuments => Set<TenderApplicationDocument>();
        public DbSet<FundingApplicationDocument> FundingApplicationDocuments => Set<FundingApplicationDocument>();
        public DbSet<RegistrationApplication> RegistrationApplications => Set<RegistrationApplication>();
        public DbSet<RegistrationProposedName> RegistrationProposedNames => Set<RegistrationProposedName>();
        public DbSet<RegistrationDirector> RegistrationDirectors => Set<RegistrationDirector>();
        public DbSet<RegistrationShareholder> RegistrationShareholders => Set<RegistrationShareholder>();
        public DbSet<RegistrationApplicationAddress> RegistrationApplicationAddresses => Set<RegistrationApplicationAddress>();
        public DbSet<RegistrationTaskTemplate> RegistrationTaskTemplates => Set<RegistrationTaskTemplate>();
        public DbSet<RegistrationApplicationTask> RegistrationApplicationTasks => Set<RegistrationApplicationTask>();
        public DbSet<RegistrationReviewEvent> RegistrationReviewEvents => Set<RegistrationReviewEvent>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<CashbookEntry> CashbookEntries => Set<CashbookEntry>();
        public DbSet<InvoiceNumberSequence> InvoiceNumberSequences => Set<InvoiceNumberSequence>();
        public DbSet<OpportunitySource> OpportunitySources => Set<OpportunitySource>();
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<OpportunityRequirement> OpportunityRequirements => Set<OpportunityRequirement>();
        public DbSet<RecommendationRun> RecommendationRuns => Set<RecommendationRun>();
        public DbSet<OpportunityMatch> OpportunityMatches => Set<OpportunityMatch>();
        public DbSet<SavedOpportunity> SavedOpportunities => Set<SavedOpportunity>();
        public DbSet<RecommendationFeedback> RecommendationFeedbackItems => Set<RecommendationFeedback>();
        public DbSet<BusinessSignal> BusinessSignals => Set<BusinessSignal>();
        public DbSet<BusinessInsight> BusinessInsights => Set<BusinessInsight>();
        public DbSet<TenderApplication> TenderApplications => Set<TenderApplication>();
        public DbSet<FundingProvider> FundingProviders => Set<FundingProvider>();
        public DbSet<FundingApplication> FundingApplications => Set<FundingApplication>();
        public DbSet<FundingReadinessAssessment> FundingReadinessAssessments => Set<FundingReadinessAssessment>();
        public DbSet<FundingReadinessAnswer> FundingReadinessAnswers => Set<FundingReadinessAnswer>();
        public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<PrivacyNoticeVersion> PrivacyNoticeVersions => Set<PrivacyNoticeVersion>();
        public DbSet<ConsentEvent> ConsentEvents => Set<ConsentEvent>();
        public DbSet<DataSubjectRequest> DataSubjectRequests => Set<DataSubjectRequest>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<DataRetentionPolicy> DataRetentionPolicies => Set<DataRetentionPolicy>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BusinessMember>()
                .HasOne(bm => bm.User)
                .WithMany(u => u.BusinessMemberships)
                .HasForeignKey(bm => bm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RegistrationApplication>()
                .HasOne(x => x.Business)
                .WithMany(x => x.RegistrationApplications)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BusinessDocument>()
                .HasKey(x => new { x.BusinessId, x.DocumentId });

            builder.Entity<CashbookEntryDocument>()
                .HasKey(x => new { x.CashbookEntryId, x.DocumentId });

            builder.Entity<FundingApplicationDocument>()
                .HasKey(x => new { x.FundingApplicationId, x.DocumentId });

            builder.Entity<InvoiceDocument>()
                .HasKey(x => new { x.InvoiceId, x.DocumentId });

            builder.Entity<RegistrationApplicationDocument>()
                .HasKey(x => new { x.RegistrationApplicationId, x.DocumentId });

            builder.Entity<TenderApplicationDocument>()
                .HasKey(x => new { x.TenderApplicationId, x.DocumentId });

            // One invoice-number sequence per business.
            builder.Entity<InvoiceNumberSequence>()
                .HasKey(x => x.BusinessId);
        }
    }
}
