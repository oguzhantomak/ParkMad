using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Parking.API.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    VehicleSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    VehicleSize = table.Column<int>(type: "int", nullable: false),
                    OccupiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OccupiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSpots_ParkingZones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "ParkingZones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ParkingZones",
                columns: new[] { "Id", "Capacity", "Name", "VehicleSize" },
                values: new object[,]
                {
                    { 1, 1, "Zone A", 0 },
                    { 2, 2, "Zone B", 1 },
                    { 3, 3, "Zone C", 2 }
                });

            migrationBuilder.InsertData(
                table: "ParkingSpots",
                columns: new[] { "Id", "IsOccupied", "OccupiedAt", "OccupiedBy", "VehicleSize", "ZoneId" },
                values: new object[,]
                {
                    { 1, false, null, null, 0, 1 },
                    { 2, false, null, null, 1, 2 },
                    { 3, false, null, null, 2, 2 },
                    { 4, true, null, null, 0, 3 },
                    { 5, true, null, null, 1, 3 },
                    { 6, false, null, null, 2, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_ZoneId",
                table: "ParkingSpots",
                column: "ZoneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingSpots");

            migrationBuilder.DropTable(
                name: "ParkingZones");
        }
    }
}
