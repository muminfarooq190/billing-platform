using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationShareLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotation_share_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_viewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_share_links", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_share_links_tenant_id_quotation_id",
                table: "quotation_share_links",
                columns: new[] { "tenant_id", "quotation_id" });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_share_links_token",
                table: "quotation_share_links",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotation_share_links");
        }
    }
}
