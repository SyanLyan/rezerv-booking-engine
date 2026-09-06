using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerv.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveBookingScheduleKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveTimetableScheduleId",
                table: "bookings",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE `bookings`
                SET `ActiveTimetableScheduleId` = `TimetableScheduleId`
                WHERE `Status` <> 3;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CustomerId_ActiveTimetableScheduleId",
                table: "bookings",
                columns: new[] { "CustomerId", "ActiveTimetableScheduleId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_bookings_CustomerId_TimetableScheduleId",
                table: "bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_bookings_CustomerId_TimetableScheduleId",
                table: "bookings",
                columns: new[] { "CustomerId", "TimetableScheduleId" },
                unique: true);

            migrationBuilder.DropIndex(
                name: "IX_bookings_CustomerId_ActiveTimetableScheduleId",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "ActiveTimetableScheduleId",
                table: "bookings");
        }
    }
}
