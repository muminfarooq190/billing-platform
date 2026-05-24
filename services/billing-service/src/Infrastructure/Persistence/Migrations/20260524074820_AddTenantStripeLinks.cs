using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantStripeLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "current_period_end",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "current_period_start",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateOnly>(
                name: "billing_period_end",
                table: "invoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "billing_period_start",
                table: "invoices",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "invoice_number",
                table: "invoices",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "payment_failure_code",
                table: "invoices",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_failure_message",
                table: "invoices",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_gateway",
                table: "invoices",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pricing_reference",
                table: "invoices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "provider_payment_id",
                table: "invoices",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "limit_merge_policy",
                table: "commercial_package_features",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "tenant_feature_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    limit_value = table.Column<int>(type: "integer", nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_feature_overrides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_user_feature_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_user_feature_assignments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_SubscriptionId_billing_period_start_billing_period~",
                table: "invoices",
                columns: new[] { "SubscriptionId", "billing_period_start", "billing_period_end" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_feature_overrides_tenant_id_feature_key_deleted_at",
                table: "tenant_feature_overrides",
                columns: new[] { "tenant_id", "feature_key", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_tenant_user_feature_assignments_lookup",
                table: "tenant_user_feature_assignments",
                columns: new[] { "tenant_id", "user_id", "feature_key", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_feature_overrides");

            migrationBuilder.DropTable(
                name: "tenant_user_feature_assignments");

            migrationBuilder.DropIndex(
                name: "IX_invoices_SubscriptionId_billing_period_start_billing_period~",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "current_period_end",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "current_period_start",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "billing_period_end",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billing_period_start",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "invoice_number",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "payment_failure_code",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "payment_failure_message",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "payment_gateway",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "pricing_reference",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "provider_payment_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "limit_merge_policy",
                table: "commercial_package_features");
        }
    }
}
