using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantTemplateThemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_template_themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateScope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HeaderHtml = table.Column<string>(type: "text", nullable: true),
                    FooterHtml = table.Column<string>(type: "text", nullable: true),
                    CustomCss = table.Column<string>(type: "text", nullable: true),
                    LogoAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    BackgroundAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_template_themes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_template_themes_TenantId_TemplateScope",
                table: "tenant_template_themes",
                columns: new[] { "TenantId", "TemplateScope" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_template_themes");
        }
    }
}
