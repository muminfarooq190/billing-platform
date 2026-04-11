using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddTenantFeatureOverridesAndLimitMergePolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "limit_merge_policy",
                table: "commercial_package_features",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Max");

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

            migrationBuilder.CreateIndex(
                name: "IX_tenant_feature_overrides_tenant_id_feature_key_deleted_at",
                table: "tenant_feature_overrides",
                columns: new[] { "tenant_id", "feature_key", "deleted_at" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_feature_overrides");

            migrationBuilder.DropColumn(
                name: "limit_merge_policy",
                table: "commercial_package_features");
        }
    }
}
