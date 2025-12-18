using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "City",
                table: "PropertyLocations");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "PropertyLocations",
                newName: "ProvinceName");

            migrationBuilder.RenameColumn(
                name: "Neighborhood",
                table: "PropertyLocations",
                newName: "CityName");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "PropertyLocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "PropertyLocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "PropertyLocations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.ProvinceId);
                    table.ForeignKey(
                        name: "FK_Provinces_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    CityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    PostalCodePrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.CityId);
                    table.ForeignKey(
                        name: "FK_Cities_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    NeighborhoodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    PostalCodePrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.NeighborhoodId);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "CountryId", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "AR", "Argentina" },
                    { 2, "UY", "Uruguay" },
                    { 3, "CL", "Chile" }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: 2,
                columns: new[] { "DurationDays", "MaxPublications", "Name" },
                values: new object[] { 30, 3, "Destacada" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: 3,
                columns: new[] { "DurationDays", "Name" },
                values: new object[] { 45, "Vip" });

            migrationBuilder.InsertData(
                table: "Provinces",
                columns: new[] { "ProvinceId", "Code", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, "BA", 1, "Buenos Aires" },
                    { 2, "CA", 1, "Catamarca" },
                    { 3, "CH", 1, "Chaco" },
                    { 4, "CT", 1, "Chubut" },
                    { 5, "CB", 1, "Córdoba" },
                    { 6, "CR", 1, "Corrientes" },
                    { 7, "ER", 1, "Entre Ríos" },
                    { 8, "FO", 1, "Formosa" },
                    { 9, "JY", 1, "Jujuy" },
                    { 10, "LP", 1, "La Pampa" },
                    { 11, "LR", 1, "La Rioja" },
                    { 12, "MZ", 1, "Mendoza" },
                    { 13, "MI", 1, "Misiones" },
                    { 14, "NQ", 1, "Neuquén" },
                    { 15, "RN", 1, "Río Negro" },
                    { 16, "SA", 1, "Salta" },
                    { 17, "SJ", 1, "San Juan" },
                    { 18, "SL", 1, "San Luis" },
                    { 19, "SC", 1, "Santa Cruz" },
                    { 20, "SF", 1, "Santa Fe" },
                    { 21, "SE", 1, "Santiago del Estero" },
                    { 22, "TF", 1, "Tierra del Fuego" },
                    { 23, "TM", 1, "Tucumán" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLocations_CityId",
                table: "PropertyLocations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLocations_NeighborhoodId",
                table: "PropertyLocations",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyLocations_ProvinceId",
                table: "PropertyLocations",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name_ProvinceId",
                table: "Cities",
                columns: new[] { "Name", "ProvinceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cities_ProvinceId",
                table: "Cities",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_CityId",
                table: "Neighborhoods",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_Name_CityId",
                table: "Neighborhoods",
                columns: new[] { "Name", "CityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_CountryId",
                table: "Provinces",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Name",
                table: "Provinces",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyLocations_Cities_CityId",
                table: "PropertyLocations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyLocations_Neighborhoods_NeighborhoodId",
                table: "PropertyLocations",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "NeighborhoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyLocations_Provinces_ProvinceId",
                table: "PropertyLocations",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "ProvinceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyLocations_Cities_CityId",
                table: "PropertyLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyLocations_Neighborhoods_NeighborhoodId",
                table: "PropertyLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PropertyLocations_Provinces_ProvinceId",
                table: "PropertyLocations");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_PropertyLocations_CityId",
                table: "PropertyLocations");

            migrationBuilder.DropIndex(
                name: "IX_PropertyLocations_NeighborhoodId",
                table: "PropertyLocations");

            migrationBuilder.DropIndex(
                name: "IX_PropertyLocations_ProvinceId",
                table: "PropertyLocations");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "PropertyLocations");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "PropertyLocations");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "PropertyLocations");

            migrationBuilder.RenameColumn(
                name: "ProvinceName",
                table: "PropertyLocations",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "CityName",
                table: "PropertyLocations",
                newName: "Neighborhood");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "PropertyLocations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: 2,
                columns: new[] { "DurationDays", "MaxPublications", "Name" },
                values: new object[] { 45, 4, "Plata" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: 3,
                columns: new[] { "DurationDays", "Name" },
                values: new object[] { 60, "Oro" });

            migrationBuilder.InsertData(
                table: "SubscriptionPlans",
                columns: new[] { "PlanId", "DurationDays", "IncludesContractManagement", "MaxPublications", "Name", "Price" },
                values: new object[] { 4, 60, true, -1, "Platino", 2000m });
        }
    }
}
