using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoParcelTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PortName = table.Column<string>(type: "TEXT", nullable: false),
                    PortCode = table.Column<string>(type: "TEXT", nullable: true),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<decimal>(type: "TEXT", nullable: true),
                    Longitude = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tankers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VesselName = table.Column<string>(type: "TEXT", nullable: false),
                    ImoNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Capacity = table.Column<decimal>(type: "TEXT", nullable: true),
                    FlagCountry = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tankers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CargoParcels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParcelNumber = table.Column<string>(type: "TEXT", nullable: false),
                    CargoType = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", nullable: false),
                    ApiGravity = table.Column<decimal>(type: "TEXT", nullable: true),
                    LoadingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    TankerId = table.Column<int>(type: "INTEGER", nullable: true),
                    OriginPortId = table.Column<int>(type: "INTEGER", nullable: true),
                    DestinationPortId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoParcels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoParcels_Ports_DestinationPortId",
                        column: x => x.DestinationPortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargoParcels_Ports_OriginPortId",
                        column: x => x.OriginPortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargoParcels_Tankers_TankerId",
                        column: x => x.TankerId,
                        principalTable: "Tankers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoParcels_DestinationPortId",
                table: "CargoParcels",
                column: "DestinationPortId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoParcels_OriginPortId",
                table: "CargoParcels",
                column: "OriginPortId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoParcels_TankerId",
                table: "CargoParcels",
                column: "TankerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargoParcels");

            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.DropTable(
                name: "Tankers");
        }
    }
}
