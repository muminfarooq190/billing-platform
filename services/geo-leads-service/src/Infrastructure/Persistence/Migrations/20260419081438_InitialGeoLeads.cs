using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoLeadsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialGeoLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "geo_area_queries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    geometry_json = table.Column<string>(type: "text", nullable: false),
                    requested_lead_types_json = table.Column<string>(type: "text", nullable: false),
                    requested_limit = table.Column<int>(type: "integer", nullable: false),
                    ranking_mode = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_area_queries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lead_source_ingestion_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    fetched_count = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead_source_ingestion_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lead_source_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_name = table.Column<string>(type: "text", nullable: false),
                    source_record_id = table.Column<string>(type: "text", nullable: false),
                    raw_name = table.Column<string>(type: "text", nullable: false),
                    raw_category = table.Column<string>(type: "text", nullable: false),
                    raw_address = table.Column<string>(type: "text", nullable: true),
                    raw_phone = table.Column<string>(type: "text", nullable: true),
                    raw_email = table.Column<string>(type: "text", nullable: true),
                    raw_website = table.Column<string>(type: "text", nullable: true),
                    raw_latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    raw_longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    raw_payload_json = table.Column<string>(type: "text", nullable: false),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead_source_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "saved_geo_areas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    geometry_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_geo_areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "geo_area_query_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    geo_area_query_id = table.Column<Guid>(type: "uuid", nullable: false),
                    geo_lead_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric", nullable: false),
                    canonical_name = table.Column<string>(type: "text", nullable: false),
                    lead_type = table.Column<string>(type: "text", nullable: false),
                    primary_email = table.Column<string>(type: "text", nullable: true),
                    primary_phone = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: false),
                    city = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    country = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    sources_json = table.Column<string>(type: "text", nullable: false),
                    reasoning_json = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geo_area_query_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_geo_area_query_results_geo_area_queries_geo_area_query_id",
                        column: x => x.geo_area_query_id,
                        principalTable: "geo_area_queries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_geo_area_query_results_geo_area_query_id",
                table: "geo_area_query_results",
                column: "geo_area_query_id");

            migrationBuilder.CreateIndex(
                name: "IX_lead_source_records_source_name_source_record_id",
                table: "lead_source_records",
                columns: new[] { "source_name", "source_record_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "geo_area_query_results");

            migrationBuilder.DropTable(
                name: "lead_source_ingestion_runs");

            migrationBuilder.DropTable(
                name: "lead_source_records");

            migrationBuilder.DropTable(
                name: "saved_geo_areas");

            migrationBuilder.DropTable(
                name: "geo_area_queries");
        }
    }
}
