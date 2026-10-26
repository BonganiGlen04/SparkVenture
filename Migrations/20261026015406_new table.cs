using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparkVenture.Migrations
{
    /// <inheritdoc />
    public partial class newtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationFinancialYearEnds",
                columns: table => new
                {
                    RegistrationApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    YearEndMonth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationFinancialYearEnds", x => x.RegistrationApplicationId);
                    table.ForeignKey(
                        name: "FK_RegistrationFinancialYearEnds_RegistrationApplications_RegistrationApplicationId",
                        column: x => x.RegistrationApplicationId,
                        principalTable: "RegistrationApplications",
                        principalColumn: "RegistrationApplicationId");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationFinancialYearEnds");
        }
    }
}
