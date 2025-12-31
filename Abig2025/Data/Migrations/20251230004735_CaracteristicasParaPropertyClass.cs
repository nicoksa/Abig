using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class CaracteristicasParaPropertyClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CoveredArea",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Expenses",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExpensesCurrency",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderConstruction",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MainRooms",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParkingSpaces",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtype",
                table: "Properties",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalArea",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Properties",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CoveredArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Expenses",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ExpensesCurrency",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsUnderConstruction",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "MainRooms",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ParkingSpaces",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Subtype",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TotalArea",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Properties");
        }
    }
}
