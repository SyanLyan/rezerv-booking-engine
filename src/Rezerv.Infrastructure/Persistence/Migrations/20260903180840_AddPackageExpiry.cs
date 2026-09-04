using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rezerv.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidityDays",
                table: "packages");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "packages",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "customer_packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    TotalCredits = table.Column<int>(type: "int", nullable: false),
                    RemainingCredits = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_packages_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_customer_packages_packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_customer_packages_CustomerId",
                table: "customer_packages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_packages_PackageId",
                table: "customer_packages",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customer_packages_CustomerId",
                table: "customer_packages");

            migrationBuilder.DropIndex(
                name: "IX_customer_packages_PackageId",
                table: "customer_packages");

            migrationBuilder.DropTable(
                name: "customer_packages");

            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "packages");

            migrationBuilder.AddColumn<int>(
                name: "ValidityDays",
                table: "packages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
