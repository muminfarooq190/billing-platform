using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommunicationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommunicationMvpPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "notifications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "document_references_json",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "notifications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "metadata_json",
                table: "notifications",
                type: "text",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "workflow_type",
                table: "notifications",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_tenant_id_idempotency_key",
                table: "notifications",
                columns: new[] { "tenant_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_tenant_id_idempotency_key",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "document_references_json",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "metadata_json",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "workflow_type",
                table: "notifications");
        }
    }
}
