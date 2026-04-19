using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace GeoLeadsService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPostGisSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Polygon>(
                name: "geometry",
                table: "saved_geo_areas",
                type: "geometry(Polygon,4326)",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "location",
                table: "lead_source_records",
                type: "geometry(Point,4326)",
                nullable: true);

            migrationBuilder.AddColumn<Polygon>(
                name: "geometry",
                table: "geo_area_queries",
                type: "geometry(Polygon,4326)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_saved_geo_areas_geometry",
                table: "saved_geo_areas",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_lead_source_records_location",
                table: "lead_source_records",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_geo_area_queries_geometry",
                table: "geo_area_queries",
                column: "geometry")
                .Annotation("Npgsql:IndexMethod", "GIST");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_saved_geo_areas_geometry",
                table: "saved_geo_areas");

            migrationBuilder.DropIndex(
                name: "IX_lead_source_records_location",
                table: "lead_source_records");

            migrationBuilder.DropIndex(
                name: "IX_geo_area_queries_geometry",
                table: "geo_area_queries");

            migrationBuilder.DropColumn(
                name: "geometry",
                table: "saved_geo_areas");

            migrationBuilder.DropColumn(
                name: "location",
                table: "lead_source_records");

            migrationBuilder.DropColumn(
                name: "geometry",
                table: "geo_area_queries");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
