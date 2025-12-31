using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoParcelTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargoParcels_Ports_DestinationPortId",
                table: "CargoParcels");

            migrationBuilder.DropForeignKey(
                name: "FK_CargoParcels_Ports_OriginPortId",
                table: "CargoParcels");

            migrationBuilder.DropIndex(
                name: "IX_CargoParcels_DestinationPortId",
                table: "CargoParcels");

            migrationBuilder.DropIndex(
                name: "IX_CargoParcels_OriginPortId",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "ApiGravity",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "DestinationPortId",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "LoadingDate",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "OriginPortId",
                table: "CargoParcels");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "CargoParcels",
                newName: "LaycanStart");

            migrationBuilder.RenameColumn(
                name: "ParcelNumber",
                table: "CargoParcels",
                newName: "LaycanEnd");

            migrationBuilder.RenameColumn(
                name: "CargoType",
                table: "CargoParcels",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CrudeGrade",
                table: "CargoParcels",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DischargePort",
                table: "CargoParcels",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoadingPort",
                table: "CargoParcels",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParcelName",
                table: "CargoParcels",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityBbls",
                table: "CargoParcels",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VesselName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ImoNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Dwt = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    VesselType = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoyageAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParcelId = table.Column<int>(type: "INTEGER", nullable: false),
                    VesselId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoadingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DischargeDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FreightRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DemurrageRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoyageAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoyageAllocations_CargoParcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "CargoParcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VoyageAllocations_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_ImoNumber",
                table: "Vessels",
                column: "ImoNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoyageAllocations_ParcelId",
                table: "VoyageAllocations",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_VoyageAllocations_VesselId",
                table: "VoyageAllocations",
                column: "VesselId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoyageAllocations");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropColumn(
                name: "CrudeGrade",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "DischargePort",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "LoadingPort",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "ParcelName",
                table: "CargoParcels");

            migrationBuilder.DropColumn(
                name: "QuantityBbls",
                table: "CargoParcels");

            migrationBuilder.RenameColumn(
                name: "LaycanStart",
                table: "CargoParcels",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "LaycanEnd",
                table: "CargoParcels",
                newName: "ParcelNumber");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "CargoParcels",
                newName: "CargoType");

            migrationBuilder.AddColumn<decimal>(
                name: "ApiGravity",
                table: "CargoParcels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DestinationPortId",
                table: "CargoParcels",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryDate",
                table: "CargoParcels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LoadingDate",
                table: "CargoParcels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CargoParcels",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginPortId",
                table: "CargoParcels",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargoParcels_DestinationPortId",
                table: "CargoParcels",
                column: "DestinationPortId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoParcels_OriginPortId",
                table: "CargoParcels",
                column: "OriginPortId");

            migrationBuilder.AddForeignKey(
                name: "FK_CargoParcels_Ports_DestinationPortId",
                table: "CargoParcels",
                column: "DestinationPortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoParcels_Ports_OriginPortId",
                table: "CargoParcels",
                column: "OriginPortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
