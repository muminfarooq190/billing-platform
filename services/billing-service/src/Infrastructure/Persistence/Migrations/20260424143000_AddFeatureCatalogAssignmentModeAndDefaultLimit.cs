using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddFeatureCatalogAssignmentModeAndDefaultLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "assignment_mode",
                table: "feature_catalog",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "TenantWide");

            migrationBuilder.AddColumn<int>(
                name: "default_assignment_limit",
                table: "feature_catalog",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assignment_mode",
                table: "feature_catalog");

            migrationBuilder.DropColumn(
                name: "default_assignment_limit",
                table: "feature_catalog");
        }
    }
}
