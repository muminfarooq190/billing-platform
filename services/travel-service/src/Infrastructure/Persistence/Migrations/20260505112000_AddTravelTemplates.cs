using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    public partial class AddTravelTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "travel_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    banner = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    accent_color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    tagline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sections_json = table.Column<string>(type: "jsonb", nullable: false),
                    seed_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_built_in = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_active_templates",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "text", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_active_templates", x => new { x.tenant_id, x.context });
                });

            migrationBuilder.CreateIndex(
                name: "IX_travel_templates_tenant_id_context_is_active",
                table: "travel_templates",
                columns: new[] { "tenant_id", "context", "is_active" });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "tenant_active_templates");
            migrationBuilder.DropTable(name: "travel_templates");
        }
    }
}
