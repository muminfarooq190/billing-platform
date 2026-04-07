using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotation_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    to_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotation_status_history_quotations_quotation_id",
                        column: x => x.quotation_id,
                        principalTable: "quotations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_status_history_quotation_id",
                table: "quotation_status_history",
                column: "quotation_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotation_status_history_tenant_id_quotation_id_created_at",
                table: "quotation_status_history",
                columns: new[] { "tenant_id", "quotation_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotation_status_history");
        }
    }
}
