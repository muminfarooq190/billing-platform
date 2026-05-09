using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantStyling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_email_template_styles",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    header_html = table.Column<string>(type: "text", nullable: false),
                    footer_html = table.Column<string>(type: "text", nullable: false),
                    accent_color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    font_family = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    custom_css_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_email_template_styles", x => x.tenant_id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_pdf_styling",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    header_layout = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    footer_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    watermark_text = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    accent_color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    margin_px = table.Column<int>(type: "integer", nullable: false),
                    custom_css_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_pdf_styling", x => x.tenant_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_email_template_styles");

            migrationBuilder.DropTable(
                name: "tenant_pdf_styling");
        }
    }
}
