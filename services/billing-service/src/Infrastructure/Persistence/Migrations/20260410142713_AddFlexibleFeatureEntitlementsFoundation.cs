using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFlexibleFeatureEntitlementsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commercial_package_features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    commercial_package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    limit_value = table.Column<int>(type: "integer", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commercial_package_features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "commercial_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    billing_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commercial_packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "feature_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    service = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_quota = table.Column<bool>(type: "boolean", nullable: false),
                    unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_catalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_subscription_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    commercial_package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_subscription_packages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commercial_package_features_commercial_package_id_feature_k~",
                table: "commercial_package_features",
                columns: new[] { "commercial_package_id", "feature_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commercial_packages_code",
                table: "commercial_packages",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_feature_catalog_feature_key",
                table: "feature_catalog",
                column: "feature_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscription_packages_tenant_id_commercial_package_i~",
                table: "tenant_subscription_packages",
                columns: new[] { "tenant_id", "commercial_package_id", "deleted_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commercial_package_features");

            migrationBuilder.DropTable(
                name: "commercial_packages");

            migrationBuilder.DropTable(
                name: "feature_catalog");

            migrationBuilder.DropTable(
                name: "tenant_subscription_packages");
        }
    }
}
