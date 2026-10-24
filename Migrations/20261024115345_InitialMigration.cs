using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparkVenture.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataRetentionPolicies",
                columns: table => new
                {
                    DataRetentionPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetentionDays = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRetentionPolicies", x => x.DataRetentionPolicyId);
                });

            migrationBuilder.CreateTable(
                name: "FundingProviders",
                columns: table => new
                {
                    FundingProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingProviders", x => x.FundingProviderId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    NotificationTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.NotificationTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "OpportunitySources",
                columns: table => new
                {
                    OpportunitySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunitySources", x => x.OpportunitySourceId);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceValues",
                columns: table => new
                {
                    ReferenceValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceValues", x => x.ReferenceValueId);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationTaskTemplates",
                columns: table => new
                {
                    RegistrationTaskTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationTaskTemplates", x => x.RegistrationTaskTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.SectorId);
                });

            migrationBuilder.CreateTable(
                name: "DataSubjectRequests",
                columns: table => new
                {
                    DataSubjectRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VerifiedContactReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSubjectRequests", x => x.DataSubjectRequestId);
                    table.ForeignKey(
                        name: "FK_DataSubjectRequests_ApplicationUser_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataSubjectRequests_ApplicationUser_RequesterUserId",
                        column: x => x.RequesterUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    NotificationPreferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.NotificationPreferenceId);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivacyNoticeVersions",
                columns: table => new
                {
                    PrivacyNoticeVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentStorageKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublishedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyNoticeVersions", x => x.PrivacyNoticeVersionId);
                    table.ForeignKey(
                        name: "FK_PrivacyNoticeVersions_ApplicationUser_PublishedByUserId",
                        column: x => x.PublishedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserProfileId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeCountBandId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnnualRevenueBandId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TradingName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.BusinessId);
                    table.ForeignKey(
                        name: "FK_Businesses_ApplicationUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Businesses_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "SectorId");
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunitySourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MinimumStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaximumStageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Revision = table.Column<int>(type: "int", nullable: false),
                    OpportunityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpensAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosesAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.OpportunityId);
                    table.ForeignKey(
                        name: "FK_Opportunities_OpportunitySources_OpportunitySourceId",
                        column: x => x.OpportunitySourceId,
                        principalTable: "OpportunitySources",
                        principalColumn: "OpportunitySourceId");
                    table.ForeignKey(
                        name: "FK_Opportunities_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "SectorId");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_ApplicationUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogs_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                });

            migrationBuilder.CreateTable(
                name: "BusinessAddresses",
                columns: table => new
                {
                    BusinessAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suburb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAddresses", x => x.BusinessAddressId);
                    table.ForeignKey(
                        name: "FK_BusinessAddresses_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessInsights",
                columns: table => new
                {
                    BusinessInsightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsightType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DismissedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInsights", x => x.BusinessInsightId);
                    table.ForeignKey(
                        name: "FK_BusinessInsights_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessInvitations",
                columns: table => new
                {
                    BusinessInvitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInvitations", x => x.BusinessInvitationId);
                    table.ForeignKey(
                        name: "FK_BusinessInvitations_ApplicationUser_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessInvitations_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessMarketLinks",
                columns: table => new
                {
                    BusinessMarketLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessMarketLinks", x => x.BusinessMarketLinkId);
                    table.ForeignKey(
                        name: "FK_BusinessMarketLinks_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessMembers",
                columns: table => new
                {
                    BusinessMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BusinessRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MembershipStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvitedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessMembers", x => x.BusinessMemberId);
                    table.ForeignKey(
                        name: "FK_BusinessMembers_ApplicationUser_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessMembers_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessMembers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessMembers_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessSignals",
                columns: table => new
                {
                    BusinessSignalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignalType = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSignals", x => x.BusinessSignalId);
                    table.ForeignKey(
                        name: "FK_BusinessSignals_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsentEvents",
                columns: table => new
                {
                    ConsentEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrivacyNoticeVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurposeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsentTextVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentEvents", x => x.ConsentEventId);
                    table.ForeignKey(
                        name: "FK_ConsentEvents_ApplicationUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsentEvents_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentEvents_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                    table.ForeignKey(
                        name: "FK_ConsentEvents_PrivacyNoticeVersions_PrivacyNoticeVersionId",
                        column: x => x.PrivacyNoticeVersionId,
                        principalTable: "PrivacyNoticeVersions",
                        principalColumn: "PrivacyNoticeVersionId");
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvgDaysToPay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LateInvoiceRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_Customers_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StorageKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectedContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    ContentHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScanStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_Documents_ApplicationUser_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    ExpenseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.ExpenseCategoryId);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                });

            migrationBuilder.CreateTable(
                name: "FundingReadinessAssessments",
                columns: table => new
                {
                    FundingReadinessAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessmentVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingReadinessAssessments", x => x.FundingReadinessAssessmentId);
                    table.ForeignKey(
                        name: "FK_FundingReadinessAssessments_ApplicationUser_AssessedByUserId",
                        column: x => x.AssessedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundingReadinessAssessments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberSequences",
                columns: table => new
                {
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BusinessId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberSequences", x => x.BusinessId);
                    table.ForeignKey(
                        name: "FK_InvoiceNumberSequences_Businesses_BusinessId1",
                        column: x => x.BusinessId1,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotificationTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTemplates_NotificationTemplateId",
                        column: x => x.NotificationTemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "NotificationTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InitiatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PaymentTransactionId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_ApplicationUser_InitiatedByUserId",
                        column: x => x.InitiatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationRuns",
                columns: table => new
                {
                    RecommendationRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TriggeredByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RunType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AlgorithmVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationRuns", x => x.RecommendationRunId);
                    table.ForeignKey(
                        name: "FK_RecommendationRuns_ApplicationUser_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RecommendationRuns_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationApplications",
                columns: table => new
                {
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedReviewerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkflowType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStepKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationApplications", x => x.RegistrationApplicationId);
                    table.ForeignKey(
                        name: "FK_RegistrationApplications_ApplicationUser_AssignedReviewerUserId",
                        column: x => x.AssignedReviewerUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrationApplications_ApplicationUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationApplications_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId");
                });

            migrationBuilder.CreateTable(
                name: "FundingApplications",
                columns: table => new
                {
                    FundingApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FundingProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingApplications", x => x.FundingApplicationId);
                    table.ForeignKey(
                        name: "FK_FundingApplications_ApplicationUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplications_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplications_FundingProviders_FundingProviderId",
                        column: x => x.FundingProviderId,
                        principalTable: "FundingProviders",
                        principalColumn: "FundingProviderId");
                    table.ForeignKey(
                        name: "FK_FundingApplications_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId");
                });

            migrationBuilder.CreateTable(
                name: "OpportunityRequirements",
                columns: table => new
                {
                    OpportunityRequirementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequirementType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityRequirements", x => x.OpportunityRequirementId);
                    table.ForeignKey(
                        name: "FK_OpportunityRequirements_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedOpportunities",
                columns: table => new
                {
                    SavedOpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedOpportunities", x => x.SavedOpportunityId);
                    table.ForeignKey(
                        name: "FK_SavedOpportunities_ApplicationUser_SavedByUserId",
                        column: x => x.SavedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedOpportunities_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedOpportunities_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderApplications",
                columns: table => new
                {
                    TenderApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmissionDeadlineUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderApplications", x => x.TenderApplicationId);
                    table.ForeignKey(
                        name: "FK_TenderApplications_ApplicationUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenderApplications_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenderApplications_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullyPaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessCredentials",
                columns: table => new
                {
                    BusinessCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CredentialTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LevelOrGrade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCredentials", x => x.BusinessCredentialId);
                    table.ForeignKey(
                        name: "FK_BusinessCredentials_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessCredentials_Documents_EvidenceDocumentId",
                        column: x => x.EvidenceDocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId");
                });

            migrationBuilder.CreateTable(
                name: "BusinessDocuments",
                columns: table => new
                {
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessDocuments", x => new { x.BusinessId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_BusinessDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingReadinessAnswers",
                columns: table => new
                {
                    FundingReadinessAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundingReadinessAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvidenceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingReadinessAnswers", x => x.FundingReadinessAnswerId);
                    table.ForeignKey(
                        name: "FK_FundingReadinessAnswers_Documents_EvidenceDocumentId",
                        column: x => x.EvidenceDocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId");
                    table.ForeignKey(
                        name: "FK_FundingReadinessAnswers_FundingReadinessAssessments_FundingReadinessAssessmentId",
                        column: x => x.FundingReadinessAssessmentId,
                        principalTable: "FundingReadinessAssessments",
                        principalColumn: "FundingReadinessAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityMatches",
                columns: table => new
                {
                    OpportunityMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecommendationRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityRevision = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MatchBand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MandatoryCriteriaPassed = table.Column<bool>(type: "bit", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoostExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AlgorithmVersion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpportunityMatches", x => x.OpportunityMatchId);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpportunityMatches_RecommendationRuns_RecommendationRunId",
                        column: x => x.RecommendationRunId,
                        principalTable: "RecommendationRuns",
                        principalColumn: "RecommendationRunId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationApplicationAddresses",
                columns: table => new
                {
                    RegistrationApplicationAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationApplicationAddresses", x => x.RegistrationApplicationAddressId);
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationAddresses_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationApplicationDocuments",
                columns: table => new
                {
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequiredEvidence = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationApplicationDocuments", x => new { x.RegistrationApplicationId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationDocuments_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationApplicationTasks",
                columns: table => new
                {
                    RegistrationApplicationTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationTaskTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TaskKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TitleSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstructionsSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationApplicationTasks", x => x.RegistrationApplicationTaskId);
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationTasks_ApplicationUser_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationTasks_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationApplicationTasks_RegistrationTaskTemplates_RegistrationTaskTemplateId",
                        column: x => x.RegistrationTaskTemplateId,
                        principalTable: "RegistrationTaskTemplates",
                        principalColumn: "RegistrationTaskTemplateId");
                });

            migrationBuilder.CreateTable(
                name: "RegistrationDirectors",
                columns: table => new
                {
                    RegistrationDirectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdentityNumberEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationDirectors", x => x.RegistrationDirectorId);
                    table.ForeignKey(
                        name: "FK_RegistrationDirectors_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationProposedNames",
                columns: table => new
                {
                    RegistrationProposedNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposedName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriorityOrder = table.Column<int>(type: "int", nullable: false),
                    AvailabilityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationProposedNames", x => x.RegistrationProposedNameId);
                    table.ForeignKey(
                        name: "FK_RegistrationProposedNames_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationReviewEvents",
                columns: table => new
                {
                    RegistrationReviewEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationReviewEvents", x => x.RegistrationReviewEventId);
                    table.ForeignKey(
                        name: "FK_RegistrationReviewEvents_ApplicationUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegistrationReviewEvents_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationShareholders",
                columns: table => new
                {
                    RegistrationShareholderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShareholderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShareClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IdentityNumberEncrypted = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationShareholders", x => x.RegistrationShareholderId);
                    table.ForeignKey(
                        name: "FK_RegistrationShareholders_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FundingApplicationDocuments",
                columns: table => new
                {
                    FundingApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequiredEvidence = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundingApplicationDocuments", x => new { x.FundingApplicationId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_FundingApplicationDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplicationDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FundingApplicationDocuments_FundingApplications_FundingApplicationId",
                        column: x => x.FundingApplicationId,
                        principalTable: "FundingApplications",
                        principalColumn: "FundingApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenderApplicationDocuments",
                columns: table => new
                {
                    TenderApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRequiredEvidence = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderApplicationDocuments", x => new { x.TenderApplicationId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_TenderApplicationDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenderApplicationDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenderApplicationDocuments_TenderApplications_TenderApplicationId",
                        column: x => x.TenderApplicationId,
                        principalTable: "TenderApplications",
                        principalColumn: "TenderApplicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDocuments",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDocuments", x => new { x.InvoiceId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_InvoiceDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDocuments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    InvoiceLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.InvoiceLineId);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    PaymentAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllocationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.PaymentAllocationId);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "PaymentTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationFeedbackItems",
                columns: table => new
                {
                    RecommendationFeedbackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityMatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FundingApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeedbackType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasonCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationFeedbackItems", x => x.RecommendationFeedbackId);
                    table.ForeignKey(
                        name: "FK_RecommendationFeedbackItems_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationFeedbackItems_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationFeedbackItems_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "OpportunityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationFeedbackItems_OpportunityMatches_OpportunityMatchId",
                        column: x => x.OpportunityMatchId,
                        principalTable: "OpportunityMatches",
                        principalColumn: "OpportunityMatchId");
                });

            migrationBuilder.CreateTable(
                name: "CashbookEntries",
                columns: table => new
                {
                    CashbookEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpenseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversesCashbookEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbookEntries", x => x.CashbookEntryId);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_ApplicationUser_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashbookEntries_CashbookEntries_ReversesCashbookEntryId",
                        column: x => x.ReversesCashbookEntryId,
                        principalTable: "CashbookEntries",
                        principalColumn: "CashbookEntryId");
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId");
                    table.ForeignKey(
                        name: "FK_CashbookEntries_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "ExpenseCategoryId");
                    table.ForeignKey(
                        name: "FK_CashbookEntries_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceId");
                    table.ForeignKey(
                        name: "FK_CashbookEntries_PaymentAllocations_PaymentAllocationId",
                        column: x => x.PaymentAllocationId,
                        principalTable: "PaymentAllocations",
                        principalColumn: "PaymentAllocationId");
                });

            migrationBuilder.CreateTable(
                name: "CashbookEntryDocuments",
                columns: table => new
                {
                    CashbookEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentPurpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashbookEntryDocuments", x => new { x.CashbookEntryId, x.DocumentId });
                    table.ForeignKey(
                        name: "FK_CashbookEntryDocuments_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashbookEntryDocuments_CashbookEntries_CashbookEntryId",
                        column: x => x.CashbookEntryId,
                        principalTable: "CashbookEntries",
                        principalColumn: "CashbookEntryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashbookEntryDocuments_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_BusinessId",
                table: "AuditLogs",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAddresses_BusinessId",
                table: "BusinessAddresses",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCredentials_BusinessId",
                table: "BusinessCredentials",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCredentials_EvidenceDocumentId",
                table: "BusinessCredentials",
                column: "EvidenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessDocuments_DocumentId",
                table: "BusinessDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_CreatedByUserId",
                table: "Businesses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_SectorId",
                table: "Businesses",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInsights_BusinessId",
                table: "BusinessInsights",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvitations_BusinessId",
                table: "BusinessInvitations",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessInvitations_InvitedByUserId",
                table: "BusinessInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMarketLinks_BusinessId",
                table: "BusinessMarketLinks",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_ApplicationUserId",
                table: "BusinessMembers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_BusinessId",
                table: "BusinessMembers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_InvitedByUserId",
                table: "BusinessMembers",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_UserId",
                table: "BusinessMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSignals_BusinessId",
                table: "BusinessSignals",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_BusinessId",
                table: "CashbookEntries",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_CreatedByUserId",
                table: "CashbookEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_CustomerId",
                table: "CashbookEntries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_ExpenseCategoryId",
                table: "CashbookEntries",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_InvoiceId",
                table: "CashbookEntries",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_PaymentAllocationId",
                table: "CashbookEntries",
                column: "PaymentAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntries_ReversesCashbookEntryId",
                table: "CashbookEntries",
                column: "ReversesCashbookEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntryDocuments_BusinessId",
                table: "CashbookEntryDocuments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CashbookEntryDocuments_DocumentId",
                table: "CashbookEntryDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentEvents_ActorUserId",
                table: "ConsentEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentEvents_BusinessId",
                table: "ConsentEvents",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentEvents_PrivacyNoticeVersionId",
                table: "ConsentEvents",
                column: "PrivacyNoticeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentEvents_UserId",
                table: "ConsentEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BusinessId",
                table: "Customers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSubjectRequests_AssignedToUserId",
                table: "DataSubjectRequests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSubjectRequests_RequesterUserId",
                table: "DataSubjectRequests",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_BusinessId",
                table: "Documents",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_BusinessId",
                table: "ExpenseCategories",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationDocuments_BusinessId",
                table: "FundingApplicationDocuments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplicationDocuments_DocumentId",
                table: "FundingApplicationDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_BusinessId",
                table: "FundingApplications",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_CreatedByUserId",
                table: "FundingApplications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_FundingProviderId",
                table: "FundingApplications",
                column: "FundingProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingApplications_OpportunityId",
                table: "FundingApplications",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingReadinessAnswers_EvidenceDocumentId",
                table: "FundingReadinessAnswers",
                column: "EvidenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingReadinessAnswers_FundingReadinessAssessmentId",
                table: "FundingReadinessAnswers",
                column: "FundingReadinessAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingReadinessAssessments_AssessedByUserId",
                table: "FundingReadinessAssessments",
                column: "AssessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FundingReadinessAssessments_BusinessId",
                table: "FundingReadinessAssessments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDocuments_BusinessId",
                table: "InvoiceDocuments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDocuments_DocumentId",
                table: "InvoiceDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceNumberSequences_BusinessId1",
                table: "InvoiceNumberSequences",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BusinessId",
                table: "Invoices",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BusinessId",
                table: "Notifications",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTemplateId",
                table: "Notifications",
                column: "NotificationTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_OpportunitySourceId",
                table: "Opportunities",
                column: "OpportunitySourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_SectorId",
                table: "Opportunities",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_BusinessId",
                table: "OpportunityMatches",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_OpportunityId",
                table: "OpportunityMatches",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatches_RecommendationRunId",
                table: "OpportunityMatches",
                column: "RecommendationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityRequirements_OpportunityId",
                table: "OpportunityRequirements",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_BusinessId",
                table: "PaymentAllocations",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_InvoiceId",
                table: "PaymentAllocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentTransactionId",
                table: "PaymentAllocations",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BusinessId",
                table: "PaymentTransactions",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_InitiatedByUserId",
                table: "PaymentTransactions",
                column: "InitiatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyNoticeVersions_PublishedByUserId",
                table: "PrivacyNoticeVersions",
                column: "PublishedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFeedbackItems_BusinessId",
                table: "RecommendationFeedbackItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFeedbackItems_OpportunityId",
                table: "RecommendationFeedbackItems",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFeedbackItems_OpportunityMatchId",
                table: "RecommendationFeedbackItems",
                column: "OpportunityMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationFeedbackItems_UserId",
                table: "RecommendationFeedbackItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationRuns_BusinessId",
                table: "RecommendationRuns",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationRuns_TriggeredByUserId",
                table: "RecommendationRuns",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationAddresses_RegistrationApplicationId",
                table: "RegistrationApplicationAddresses",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationDocuments_BusinessId",
                table: "RegistrationApplicationDocuments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationDocuments_DocumentId",
                table: "RegistrationApplicationDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplications_AssignedReviewerUserId",
                table: "RegistrationApplications",
                column: "AssignedReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplications_BusinessId",
                table: "RegistrationApplications",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplications_CreatedByUserId",
                table: "RegistrationApplications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationTasks_AssignedToUserId",
                table: "RegistrationApplicationTasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationTasks_RegistrationApplicationId",
                table: "RegistrationApplicationTasks",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationApplicationTasks_RegistrationTaskTemplateId",
                table: "RegistrationApplicationTasks",
                column: "RegistrationTaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDirectors_RegistrationApplicationId",
                table: "RegistrationDirectors",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationProposedNames_RegistrationApplicationId",
                table: "RegistrationProposedNames",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationReviewEvents_ActorUserId",
                table: "RegistrationReviewEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationReviewEvents_RegistrationApplicationId",
                table: "RegistrationReviewEvents",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationShareholders_RegistrationApplicationId",
                table: "RegistrationShareholders",
                column: "RegistrationApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_BusinessId",
                table: "SavedOpportunities",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_OpportunityId",
                table: "SavedOpportunities",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_SavedByUserId",
                table: "SavedOpportunities",
                column: "SavedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderApplicationDocuments_BusinessId",
                table: "TenderApplicationDocuments",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderApplicationDocuments_DocumentId",
                table: "TenderApplicationDocuments",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderApplications_BusinessId",
                table: "TenderApplications",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderApplications_CreatedByUserId",
                table: "TenderApplications",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenderApplications_OpportunityId",
                table: "TenderApplications",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessAddresses");

            migrationBuilder.DropTable(
                name: "BusinessCredentials");

            migrationBuilder.DropTable(
                name: "BusinessDocuments");

            migrationBuilder.DropTable(
                name: "BusinessInsights");

            migrationBuilder.DropTable(
                name: "BusinessInvitations");

            migrationBuilder.DropTable(
                name: "BusinessMarketLinks");

            migrationBuilder.DropTable(
                name: "BusinessMembers");

            migrationBuilder.DropTable(
                name: "BusinessSignals");

            migrationBuilder.DropTable(
                name: "CashbookEntryDocuments");

            migrationBuilder.DropTable(
                name: "ConsentEvents");

            migrationBuilder.DropTable(
                name: "DataRetentionPolicies");

            migrationBuilder.DropTable(
                name: "DataSubjectRequests");

            migrationBuilder.DropTable(
                name: "FundingApplicationDocuments");

            migrationBuilder.DropTable(
                name: "FundingReadinessAnswers");

            migrationBuilder.DropTable(
                name: "InvoiceDocuments");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "InvoiceNumberSequences");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OpportunityRequirements");

            migrationBuilder.DropTable(
                name: "RecommendationFeedbackItems");

            migrationBuilder.DropTable(
                name: "ReferenceValues");

            migrationBuilder.DropTable(
                name: "RegistrationApplicationAddresses");

            migrationBuilder.DropTable(
                name: "RegistrationApplicationDocuments");

            migrationBuilder.DropTable(
                name: "RegistrationApplicationTasks");

            migrationBuilder.DropTable(
                name: "RegistrationDirectors");

            migrationBuilder.DropTable(
                name: "RegistrationProposedNames");

            migrationBuilder.DropTable(
                name: "RegistrationReviewEvents");

            migrationBuilder.DropTable(
                name: "RegistrationShareholders");

            migrationBuilder.DropTable(
                name: "SavedOpportunities");

            migrationBuilder.DropTable(
                name: "TenderApplicationDocuments");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CashbookEntries");

            migrationBuilder.DropTable(
                name: "PrivacyNoticeVersions");

            migrationBuilder.DropTable(
                name: "FundingApplications");

            migrationBuilder.DropTable(
                name: "FundingReadinessAssessments");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "OpportunityMatches");

            migrationBuilder.DropTable(
                name: "RegistrationTaskTemplates");

            migrationBuilder.DropTable(
                name: "RegistrationApplications");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "TenderApplications");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "FundingProviders");

            migrationBuilder.DropTable(
                name: "RecommendationRuns");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "OpportunitySources");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropTable(
                name: "Sectors");
        }
    }
}
