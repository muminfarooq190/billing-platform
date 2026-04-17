using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftTripConcepts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "draft_trip_concepts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    travel_inquiry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    destination = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    travellers = table.Column<int>(type: "integer", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    budget_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    concept_status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    option_label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_trip_concepts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "draft_trip_concept_days",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    overnight_location = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    draft_trip_concept_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_trip_concept_days", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_trip_concept_days_draft_trip_concepts_draft_trip_conc~",
                        column: x => x.draft_trip_concept_id,
                        principalTable: "draft_trip_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_draft_trip_concept_days_draft_trip_concept_id",
                table: "draft_trip_concept_days",
                column: "draft_trip_concept_id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_trip_concepts_tenant_id_concept_status_updated_at",
                table: "draft_trip_concepts",
                columns: new[] { "tenant_id", "concept_status", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_draft_trip_concepts_tenant_id_travel_inquiry_id_is_primary",
                table: "draft_trip_concepts",
                columns: new[] { "tenant_id", "travel_inquiry_id", "is_primary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "draft_trip_concept_days");

            migrationBuilder.DropTable(
                name: "draft_trip_concepts");
        }
    }
}
