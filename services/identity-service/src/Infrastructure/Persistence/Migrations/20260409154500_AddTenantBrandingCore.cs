using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    public partial class AddTenantBrandingCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_branding",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SecondaryColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AccentColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TextColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    BackgroundColor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ThemeMode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DefaultFontFamily = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SupportEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SupportPhone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tagline = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_branding", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "tenant_brand_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    AltText = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_brand_assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_brand_assets_TenantId_AssetType_IsActive",
                table: "tenant_brand_assets",
                columns: new[] { "TenantId", "AssetType", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tenant_brand_assets");
            migrationBuilder.DropTable(name: "tenant_branding");
        }
    }
}
