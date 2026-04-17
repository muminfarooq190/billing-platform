using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelInquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmationDeadline",
                table: "booking_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                table: "booking_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IssuedAt",
                table: "booking_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "booking_change_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_type = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_booking_change_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "travel_inquiries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    full_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    whatsapp_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    departure_city = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    destination = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    travel_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    return_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_date_flexible = table.Column<bool>(type: "boolean", nullable: false),
                    travellers = table.Column<int>(type: "integer", nullable: true),
                    budget_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    budget_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    customer_message = table.Column<string>(type: "text", nullable: true),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    qualified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    contacted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disqualified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    converted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    converted_contact_id = table.Column<Guid>(type: "uuid", nullable: true),
                    converted_quotation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_inquiries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "travel_inquiry_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    travel_inquiry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    to_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_inquiry_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_travel_inquiry_status_history_travel_inquiries_travel_inqui~",
                        column: x => x.travel_inquiry_id,
                        principalTable: "travel_inquiries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_change_requests_booking_id_status_requested_at",
                table: "booking_change_requests",
                columns: new[] { "booking_id", "status", "requested_at" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_inquiries_tenant_id_assigned_to_user_id_status",
                table: "travel_inquiries",
                columns: new[] { "tenant_id", "assigned_to_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_inquiries_tenant_id_source_created_at",
                table: "travel_inquiries",
                columns: new[] { "tenant_id", "source", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_inquiries_tenant_id_status_created_at",
                table: "travel_inquiries",
                columns: new[] { "tenant_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_inquiry_status_history_tenant_id_travel_inquiry_id_c~",
                table: "travel_inquiry_status_history",
                columns: new[] { "tenant_id", "travel_inquiry_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_travel_inquiry_status_history_travel_inquiry_id",
                table: "travel_inquiry_status_history",
                column: "travel_inquiry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_change_requests");

            migrationBuilder.DropTable(
                name: "travel_inquiry_status_history");

            migrationBuilder.DropTable(
                name: "travel_inquiries");

            migrationBuilder.DropColumn(
                name: "ConfirmationDeadline",
                table: "booking_items");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "booking_items");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "booking_items");
        }
    }
}
