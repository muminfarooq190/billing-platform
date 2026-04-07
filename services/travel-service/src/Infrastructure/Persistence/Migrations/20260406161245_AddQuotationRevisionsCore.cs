using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotationRevisionsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "accepted_revision_id",
                table: "quotations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "current_revision_number",
                table: "quotations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expired_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_sent_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_viewed_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "rejected_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "share_token",
                table: "quotations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "share_token_expires_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "quotation_revisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_number = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    customer_contact_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    destination = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    travel_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    return_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    travellers = table.Column<int>(type: "integer", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    visible_notes = table.Column<string>(type: "text", nullable: false),
                    internal_notes = table.Column<string>(type: "text", nullable: false),
                    valid_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_revisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotation_revisions_quotations_quotation_id",
                        column: x => x.quotation_id,
                        principalTable: "quotations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quotation_revision_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quotation_revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quotation_revision_line_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_quotation_revision_line_items_quotation_revisions_quotation~",
                        column: x => x.quotation_revision_id,
                        principalTable: "quotation_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotation_revision_line_items_quotation_revision_id",
                table: "quotation_revision_line_items",
                column: "quotation_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_quotation_revisions_quotation_id_revision_number",
                table: "quotation_revisions",
                columns: new[] { "quotation_id", "revision_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quotation_revisions_tenant_id_quotation_id_revision_number",
                table: "quotation_revisions",
                columns: new[] { "tenant_id", "quotation_id", "revision_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quotation_revision_line_items");

            migrationBuilder.DropTable(
                name: "quotation_revisions");

            migrationBuilder.DropColumn(
                name: "accepted_revision_id",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "current_revision_number",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "expired_at",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "last_sent_at",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "last_viewed_at",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "rejected_at",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "share_token",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "share_token_expires_at",
                table: "quotations");
        }
    }
}
