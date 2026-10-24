using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparkVenture.Migrations
{
    /// <inheritdoc />
    public partial class FixIdentityApplicationUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_ApplicationUser_ActorUserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_ApplicationUser_CreatedByUserId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessInvitations_ApplicationUser_InvitedByUserId",
                table: "BusinessInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessMembers_ApplicationUser_InvitedByUserId",
                table: "BusinessMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessMembers_ApplicationUser_UserId",
                table: "BusinessMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_ApplicationUserId",
                table: "BusinessMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CashbookEntries_ApplicationUser_CreatedByUserId",
                table: "CashbookEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentEvents_ApplicationUser_ActorUserId",
                table: "ConsentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentEvents_ApplicationUser_UserId",
                table: "ConsentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DataSubjectRequests_ApplicationUser_AssignedToUserId",
                table: "DataSubjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DataSubjectRequests_ApplicationUser_RequesterUserId",
                table: "DataSubjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_ApplicationUser_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingApplications_ApplicationUser_CreatedByUserId",
                table: "FundingApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingReadinessAssessments_ApplicationUser_AssessedByUserId",
                table: "FundingReadinessAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationPreferences_ApplicationUser_UserId",
                table: "NotificationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ApplicationUser_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_ApplicationUser_InitiatedByUserId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivacyNoticeVersions_ApplicationUser_PublishedByUserId",
                table: "PrivacyNoticeVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_RecommendationFeedbackItems_ApplicationUser_UserId",
                table: "RecommendationFeedbackItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RecommendationRuns_ApplicationUser_TriggeredByUserId",
                table: "RecommendationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplications_ApplicationUser_AssignedReviewerUserId",
                table: "RegistrationApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplications_ApplicationUser_CreatedByUserId",
                table: "RegistrationApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplicationTasks_ApplicationUser_AssignedToUserId",
                table: "RegistrationApplicationTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationReviewEvents_ApplicationUser_ActorUserId",
                table: "RegistrationReviewEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedOpportunities_ApplicationUser_SavedByUserId",
                table: "SavedOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_TenderApplications_ApplicationUser_CreatedByUserId",
                table: "TenderApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_ApplicationUser_UserId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropIndex(
                name: "IX_BusinessMembers_ApplicationUserId",
                table: "BusinessMembers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BusinessMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_AspNetUsers_CreatedByUserId",
                table: "Businesses",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessInvitations_AspNetUsers_InvitedByUserId",
                table: "BusinessInvitations",
                column: "InvitedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_InvitedByUserId",
                table: "BusinessMembers",
                column: "InvitedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_UserId",
                table: "BusinessMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashbookEntries_AspNetUsers_CreatedByUserId",
                table: "CashbookEntries",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentEvents_AspNetUsers_ActorUserId",
                table: "ConsentEvents",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentEvents_AspNetUsers_UserId",
                table: "ConsentEvents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSubjectRequests_AspNetUsers_AssignedToUserId",
                table: "DataSubjectRequests",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSubjectRequests_AspNetUsers_RequesterUserId",
                table: "DataSubjectRequests",
                column: "RequesterUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingApplications_AspNetUsers_CreatedByUserId",
                table: "FundingApplications",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingReadinessAssessments_AspNetUsers_AssessedByUserId",
                table: "FundingReadinessAssessments",
                column: "AssessedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationPreferences_AspNetUsers_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_AspNetUsers_InitiatedByUserId",
                table: "PaymentTransactions",
                column: "InitiatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivacyNoticeVersions_AspNetUsers_PublishedByUserId",
                table: "PrivacyNoticeVersions",
                column: "PublishedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendationFeedbackItems_AspNetUsers_UserId",
                table: "RecommendationFeedbackItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendationRuns_AspNetUsers_TriggeredByUserId",
                table: "RecommendationRuns",
                column: "TriggeredByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplications_AspNetUsers_AssignedReviewerUserId",
                table: "RegistrationApplications",
                column: "AssignedReviewerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplications_AspNetUsers_CreatedByUserId",
                table: "RegistrationApplications",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplicationTasks_AspNetUsers_AssignedToUserId",
                table: "RegistrationApplicationTasks",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationReviewEvents_AspNetUsers_ActorUserId",
                table: "RegistrationReviewEvents",
                column: "ActorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedOpportunities_AspNetUsers_SavedByUserId",
                table: "SavedOpportunities",
                column: "SavedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenderApplications_AspNetUsers_CreatedByUserId",
                table: "TenderApplications",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_ActorUserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_AspNetUsers_CreatedByUserId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessInvitations_AspNetUsers_InvitedByUserId",
                table: "BusinessInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_InvitedByUserId",
                table: "BusinessMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_UserId",
                table: "BusinessMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_CashbookEntries_AspNetUsers_CreatedByUserId",
                table: "CashbookEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentEvents_AspNetUsers_ActorUserId",
                table: "ConsentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentEvents_AspNetUsers_UserId",
                table: "ConsentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DataSubjectRequests_AspNetUsers_AssignedToUserId",
                table: "DataSubjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DataSubjectRequests_AspNetUsers_RequesterUserId",
                table: "DataSubjectRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_UploadedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingApplications_AspNetUsers_CreatedByUserId",
                table: "FundingApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_FundingReadinessAssessments_AspNetUsers_AssessedByUserId",
                table: "FundingReadinessAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationPreferences_AspNetUsers_UserId",
                table: "NotificationPreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_AspNetUsers_InitiatedByUserId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivacyNoticeVersions_AspNetUsers_PublishedByUserId",
                table: "PrivacyNoticeVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_RecommendationFeedbackItems_AspNetUsers_UserId",
                table: "RecommendationFeedbackItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RecommendationRuns_AspNetUsers_TriggeredByUserId",
                table: "RecommendationRuns");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplications_AspNetUsers_AssignedReviewerUserId",
                table: "RegistrationApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplications_AspNetUsers_CreatedByUserId",
                table: "RegistrationApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationApplicationTasks_AspNetUsers_AssignedToUserId",
                table: "RegistrationApplicationTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationReviewEvents_AspNetUsers_ActorUserId",
                table: "RegistrationReviewEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedOpportunities_AspNetUsers_SavedByUserId",
                table: "SavedOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_TenderApplications_AspNetUsers_CreatedByUserId",
                table: "TenderApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_UserId",
                table: "UserProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationUserId",
                table: "BusinessMembers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessMembers_ApplicationUserId",
                table: "BusinessMembers",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_ApplicationUser_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_ApplicationUser_CreatedByUserId",
                table: "Businesses",
                column: "CreatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessInvitations_ApplicationUser_InvitedByUserId",
                table: "BusinessInvitations",
                column: "InvitedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessMembers_ApplicationUser_InvitedByUserId",
                table: "BusinessMembers",
                column: "InvitedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessMembers_ApplicationUser_UserId",
                table: "BusinessMembers",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessMembers_AspNetUsers_ApplicationUserId",
                table: "BusinessMembers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CashbookEntries_ApplicationUser_CreatedByUserId",
                table: "CashbookEntries",
                column: "CreatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentEvents_ApplicationUser_ActorUserId",
                table: "ConsentEvents",
                column: "ActorUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentEvents_ApplicationUser_UserId",
                table: "ConsentEvents",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSubjectRequests_ApplicationUser_AssignedToUserId",
                table: "DataSubjectRequests",
                column: "AssignedToUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataSubjectRequests_ApplicationUser_RequesterUserId",
                table: "DataSubjectRequests",
                column: "RequesterUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_ApplicationUser_UploadedByUserId",
                table: "Documents",
                column: "UploadedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingApplications_ApplicationUser_CreatedByUserId",
                table: "FundingApplications",
                column: "CreatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FundingReadinessAssessments_ApplicationUser_AssessedByUserId",
                table: "FundingReadinessAssessments",
                column: "AssessedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationPreferences_ApplicationUser_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ApplicationUser_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_ApplicationUser_InitiatedByUserId",
                table: "PaymentTransactions",
                column: "InitiatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivacyNoticeVersions_ApplicationUser_PublishedByUserId",
                table: "PrivacyNoticeVersions",
                column: "PublishedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendationFeedbackItems_ApplicationUser_UserId",
                table: "RecommendationFeedbackItems",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendationRuns_ApplicationUser_TriggeredByUserId",
                table: "RecommendationRuns",
                column: "TriggeredByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplications_ApplicationUser_AssignedReviewerUserId",
                table: "RegistrationApplications",
                column: "AssignedReviewerUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplications_ApplicationUser_CreatedByUserId",
                table: "RegistrationApplications",
                column: "CreatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationApplicationTasks_ApplicationUser_AssignedToUserId",
                table: "RegistrationApplicationTasks",
                column: "AssignedToUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationReviewEvents_ApplicationUser_ActorUserId",
                table: "RegistrationReviewEvents",
                column: "ActorUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedOpportunities_ApplicationUser_SavedByUserId",
                table: "SavedOpportunities",
                column: "SavedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenderApplications_ApplicationUser_CreatedByUserId",
                table: "TenderApplications",
                column: "CreatedByUserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_ApplicationUser_UserId",
                table: "UserProfiles",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");
        }
    }
}
