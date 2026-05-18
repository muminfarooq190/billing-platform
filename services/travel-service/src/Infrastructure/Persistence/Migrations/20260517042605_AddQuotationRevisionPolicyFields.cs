using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationRevisionPolicyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_policy",
                table: "quotation_revisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "exclusions_json",
                table: "quotation_revisions",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "inclusions_json",
                table: "quotation_revisions",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "payment_terms",
                table: "quotation_revisions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cancellation_policy",
                table: "quotation_revisions");

            migrationBuilder.DropColumn(
                name: "exclusions_json",
                table: "quotation_revisions");

            migrationBuilder.DropColumn(
                name: "inclusions_json",
                table: "quotation_revisions");

            migrationBuilder.DropColumn(
                name: "payment_terms",
                table: "quotation_revisions");
        }
    }
}
