using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationApprovalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quotation_approval_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    margin_percent = table.Column<decimal>(type: "numeric", nullable: true),
                    discount_percent = table.Column<decimal>(type: "numeric", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    decision_reason = table.Column<string>(type: "text", nullable: true),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_approval_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_approval_requests_quotation_id_status_requested_at",
                table: "quotation_approval_requests",
                columns: new[] { "quotation_id", "status", "requested_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotation_approval_requests");
        }
    }
}
