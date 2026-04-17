using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingLinkToItineraries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "booking_id",
                table: "itineraries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_itineraries_booking_id",
                table: "itineraries",
                column: "booking_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_itineraries_booking_id",
                table: "itineraries");

            migrationBuilder.DropColumn(
                name: "booking_id",
                table: "itineraries");
        }
    }
}
