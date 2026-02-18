using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assignment_Example_HU.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGameIdFromBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Games_GameId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Games_BookingId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_GameId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Games_BookingId",
                table: "Games",
                column: "BookingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Games_BookingId",
                table: "Games");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "Bookings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_BookingId",
                table: "Games",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_GameId",
                table: "Bookings",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Games_GameId",
                table: "Bookings",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "GameId");
        }
    }
}
