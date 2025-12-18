using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 201);

            migrationBuilder.DeleteData(
                table: "Neighborhoods",
                keyColumn: "NeighborhoodId",
                keyValue: 202);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityId",
                keyValue: 100);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Provinces",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Neighborhoods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Countries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Cities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: 1,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: 2,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "CountryId",
                keyValue: 3,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 1,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 2,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 3,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 4,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 5,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 6,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 7,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 8,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 9,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 10,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 11,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 12,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 13,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 14,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 15,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 16,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 17,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 18,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 19,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 20,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 21,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 22,
                column: "isActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Provinces",
                keyColumn: "ProvinceId",
                keyValue: 23,
                column: "isActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Neighborhoods");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Cities");

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "CityId", "Name", "PostalCodePrefix", "ProvinceId" },
                values: new object[,]
                {
                    { 7, "Mendoza Capital", "M5500", 12 },
                    { 8, "Godoy Cruz", "M5501", 12 },
                    { 9, "Guaymallén", "M5521", 12 },
                    { 10, "Las Heras", "M5539", 12 },
                    { 11, "Córdoba Capital", "X5000", 5 },
                    { 12, "Villa María", "X5900", 5 },
                    { 13, "Río Cuarto", "X5800", 5 },
                    { 100, "Ciudad de Buenos Aires", "C1000", 1 }
                });

            migrationBuilder.InsertData(
                table: "Neighborhoods",
                columns: new[] { "NeighborhoodId", "CityId", "Name", "PostalCodePrefix" },
                values: new object[,]
                {
                    { 1, 100, "Palermo", "C1425" },
                    { 2, 100, "Recoleta", "C1113" },
                    { 3, 100, "Belgrano", "C1428" },
                    { 4, 100, "Núñez", "C1429" },
                    { 5, 100, "Colegiales", "C1426" },
                    { 201, 7, "Centro", "M5500" },
                    { 202, 7, "Quinta Sección", "M5501" }
                });
        }
    }
}
