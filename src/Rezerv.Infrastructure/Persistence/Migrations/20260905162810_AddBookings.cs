using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerv.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TimetableScheduleId = table.Column<int>(type: "int", nullable: false),
                    CustomerPackageId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookings_customer_packages_CustomerPackageId",
                        column: x => x.CustomerPackageId,
                        principalTable: "customer_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bookings_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bookings_timetable_schedules_TimetableScheduleId",
                        column: x => x.TimetableScheduleId,
                        principalTable: "timetable_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CustomerId_TimetableScheduleId",
                table: "bookings",
                columns: new[] { "CustomerId", "TimetableScheduleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CustomerPackageId",
                table: "bookings",
                column: "CustomerPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_TimetableScheduleId_Status_CreatedAtUtc",
                table: "bookings",
                columns: new[] { "TimetableScheduleId", "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");
        }
    }
}
