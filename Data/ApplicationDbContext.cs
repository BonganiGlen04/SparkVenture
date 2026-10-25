using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using System;
using System.Linq;
using System.Reflection.Emit;


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
        public DbSet<DocumentContent> DocumentContents => Set<DocumentContent>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);  

            builder.Entity<DocumentContent>(entity =>
            {
                entity.ToTable("DocumentContents");
                entity.HasKey(x => x.DocumentId);
                entity.Property(x => x.FileBytes).HasColumnType("varbinary(max)").IsRequired();
                entity.HasOne<Document>().WithOne()
                      .HasForeignKey<DocumentContent>(x => x.DocumentId)
                      .OnDelete(DeleteBehavior.NoAction);
            });


            // Link entities use their own single-column primary keys.
            // These unique indexes prevent duplicate links without composite primary keys.
            builder.Entity<BusinessDocument>().HasKey(x => x.BusinessDocumentId);
            builder.Entity<BusinessDocument>()
                .HasIndex(x => new { x.BusinessId, x.DocumentId }).IsUnique();

            builder.Entity<CashbookEntryDocument>().HasKey(x => x.CashbookEntryDocumentId);
            builder.Entity<CashbookEntryDocument>()
                .HasIndex(x => new { x.CashbookEntryId, x.DocumentId }).IsUnique();

            builder.Entity<FundingApplicationDocument>().HasKey(x => x.FundingApplicationDocumentId);
            builder.Entity<FundingApplicationDocument>()
                .HasIndex(x => new { x.FundingApplicationId, x.DocumentId }).IsUnique();

            builder.Entity<InvoiceDocument>().HasKey(x => x.InvoiceDocumentId);
            builder.Entity<InvoiceDocument>()
                .HasIndex(x => new { x.InvoiceId, x.DocumentId }).IsUnique();

            builder.Entity<RegistrationApplicationDocument>().HasKey(x => x.RegistrationApplicationDocumentId);
            builder.Entity<RegistrationApplicationDocument>()
                .HasIndex(x => new { x.RegistrationApplicationId, x.DocumentId }).IsUnique();

            builder.Entity<TenderApplicationDocument>().HasKey(x => x.TenderApplicationDocumentId);
            builder.Entity<TenderApplicationDocument>()
                .HasIndex(x => new { x.TenderApplicationId, x.DocumentId }).IsUnique();

            // Exactly one invoice-number sequence per business. Explicitly bind the FK
            // because this relationship has no inverse collection navigation on Business.
            builder.Entity<InvoiceNumberSequence>().HasKey(x => x.InvoiceNumberSequenceId);
            builder.Entity<InvoiceNumberSequence>()
                .HasOne(x => x.Business)
                .WithMany()
                .HasForeignKey(x => x.BusinessId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<InvoiceNumberSequence>()
                .HasIndex(x => x.BusinessId).IsUnique();

            // A profile is an optional one-to-one extension of an Identity user.
            // UserId is unique so one user cannot own multiple profiles.
            builder.Entity<UserProfile>()
                .HasOne(x => x.User)
                .WithOne(x => x.Profile)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<UserProfile>()
                .HasIndex(x => x.UserId).IsUnique();

            // Configure SQL Server rowversion columns as concurrency tokens.
            builder.Entity<Business>().Property(x => x.RowVersion).IsRowVersion();
            builder.Entity<Invoice>().Property(x => x.RowVersion).IsRowVersion();
            builder.Entity<InvoiceNumberSequence>().Property(x => x.RowVersion).IsRowVersion();

            // Resolve relationships with multiple navigations explicitly.
            builder.Entity<RegistrationApplication>()
                .HasOne(x => x.Business)
                .WithMany(x => x.RegistrationApplications)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FundingApplication>()
                .HasOne(x => x.Business)
                .WithMany(x => x.FundingApplications)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.NoAction);

            // A business member has two separate user relationships. Map both explicitly
            // so EF Core does not invent a shadow FK for either navigation.
            builder.Entity<BusinessMember>()
                .HasOne(x => x.User)
                .WithMany(x => x.BusinessMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BusinessMember>()
                .HasOne(x => x.InvitedByUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Explicit precision prevents EF Core's default decimal mapping from silently
            // rounding/truncating values. Monetary/quantity values get four decimal places;
            // scores, rates, weights, and percentages use a narrower range.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(19);
                        property.SetScale(4);
                    }
                }
            }

            builder.Entity<BusinessSignal>().Property(x => x.Weight).HasPrecision(9, 4);
            builder.Entity<Customer>().Property(x => x.AvgDaysToPay).HasPrecision(9, 4);
            builder.Entity<Customer>().Property(x => x.LateInvoiceRate).HasPrecision(9, 4);
            builder.Entity<FundingReadinessAnswer>().Property(x => x.Score).HasPrecision(9, 4);
            builder.Entity<FundingReadinessAssessment>().Property(x => x.OverallScore).HasPrecision(9, 4);
            builder.Entity<OpportunityMatch>().Property(x => x.Score).HasPrecision(9, 4);
            builder.Entity<RegistrationShareholder>().Property(x => x.SharePercentage).HasPrecision(9, 4);

            // Keep all FK constraints, but disable database-side cascading globally. SQL Server
            // rejects multiple cascade paths; dependent cleanup must be handled deliberately by
            // application workflows. Apply after the explicit mappings above.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
                }
            }
        }
    }
}
