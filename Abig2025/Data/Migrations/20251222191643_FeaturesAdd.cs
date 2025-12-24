using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeaturesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "PropertyFeatures");

            migrationBuilder.RenameColumn(
                name: "DisplayOrder",
                table: "PropertyFeatures",
                newName: "FeatureDefinitionId");

            migrationBuilder.RenameColumn(
                name: "FeatureId",
                table: "PropertyFeatures",
                newName: "PropertyFeatureId");

            migrationBuilder.CreateTable(
                name: "FeatureDefinitions",
                columns: table => new
                {
                    FeatureDefinitionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureDefinitions", x => x.FeatureDefinitionId);
                });

            migrationBuilder.InsertData(
                table: "FeatureDefinitions",
                columns: new[] { "FeatureDefinitionId", "DisplayName", "DisplayOrder", "Group", "Icon", "IsActive", "Key", "Scope", "ValueType" },
                values: new object[,]
                {
                    { 1, "Permite mascotas", 1, "Mascotas", "bi bi-heart", true, "PermiteMascotas", 0, 1 },
                    { 2, "Acceso discapacitados", 10, "Generales", "bi bi-person-bounding-box", true, "AccesoDiscapacitados", 0, 1 },
                    { 3, "Apto profesional", 11, "Generales", "bi bi-briefcase", true, "AptoProfesional", 0, 1 },
                    { 4, "Uso comercial", 12, "Generales", "bi bi-shop", true, "UsoComercial", 0, 1 },
                    { 5, "Parrilla", 20, "Amenities", "bi bi-fire", true, "Parrilla", 1, 1 },
                    { 6, "Quincho", 21, "Amenities", "bi bi-fire", true, "Quincho", 1, 1 },
                    { 7, "Gimnasio", 22, "Amenities", "bi bi-activity", true, "Gimnasio", 1, 1 },
                    { 8, "Solarium", 23, "Amenities", "bi bi-sun", true, "Solarium", 1, 1 },
                    { 9, "Hidromasaje", 24, "Amenities", "bi bi-droplet", true, "Hidromasaje", 1, 1 },
                    { 10, "Sauna", 25, "Amenities", "bi bi-thermometer-sun", true, "Sauna", 1, 1 },
                    { 11, "Pista deportiva", 26, "Amenities", "bi bi-trophy", true, "PistaDeportiva", 1, 1 },
                    { 12, "Sala de juegos", 27, "Amenities", "bi bi-joystick", true, "SalaJuegos", 1, 1 },
                    { 13, "Aire acondicionado", 30, "Confort", "bi bi-snow", true, "AireAcondicionado", 0, 1 },
                    { 14, "Caldera", 31, "Confort", "bi bi-thermometer-high", true, "Caldera", 0, 1 },
                    { 15, "Termotanque", 32, "Confort", "bi bi-droplet-half", true, "Termotanque", 0, 1 },
                    { 16, "Lavavajillas", 33, "Confort", "bi bi-cup-straw", true, "Lavavajillas", 0, 1 },
                    { 17, "Alarma", 34, "Confort", "bi bi-shield-lock", true, "Alarma", 0, 1 },
                    { 18, "Amueblado", 40, "Interiores", "bi bi-house-door", true, "Amueblado", 0, 1 },
                    { 19, "Cocina equipada", 41, "Interiores", "bi bi-egg-fried", true, "CocinaEquipada", 0, 1 },
                    { 20, "Lavadero", 42, "Interiores", "bi bi-bucket", true, "Lavadero", 0, 1 },
                    { 21, "Baulera", 43, "Interiores", "bi bi-box", true, "Baulera", 0, 1 },
                    { 22, "Toilette", 44, "Interiores", "bi bi-door-open", true, "Toilette", 0, 1 },
                    { 23, "Balcón", 45, "Interiores", "bi bi-layout-sidebar-inset", true, "Balcon", 0, 1 },
                    { 24, "Terraza", 46, "Interiores", "bi bi-layers", true, "Terraza", 0, 1 },
                    { 25, "Patio", 47, "Interiores", "bi bi-tree", true, "Patio", 0, 1 },
                    { 26, "Jardín", 48, "Interiores", "bi bi-flower1", true, "Jardin", 0, 1 },
                    { 27, "Internet / WiFi", 50, "Servicios", "bi bi-wifi", true, "InternetWifi", 0, 1 },
                    { 28, "Ascensor", 51, "Servicios", "bi bi-arrow-up", true, "Ascensor", 1, 1 },
                    { 29, "Vigilancia", 52, "Servicios", "bi bi-eye", true, "Vigilancia", 0, 1 },
                    { 30, "Servicio de limpieza", 53, "Servicios", "bi bi-stars", true, "Limpieza", 0, 1 },
                    { 31, "Agua", 54, "Servicios", "bi bi-droplet", true, "Agua", 0, 1 },
                    { 32, "Electricidad", 55, "Servicios", "bi bi-lightning", true, "Electricidad", 0, 1 },
                    { 33, "Molino", 60, "Campo", "bi bi-wind", true, "Campo_Molino", 2, 1 },
                    { 34, "Manga", 61, "Campo", "bi bi-diagram-3", true, "Campo_Manga", 2, 1 },
                    { 35, "Cargador", 62, "Campo", "bi bi-truck", true, "Campo_Cargador", 2, 1 },
                    { 36, "Alambrado", 63, "Campo", "bi bi-border", true, "Campo_Alambrado", 2, 1 },
                    { 37, "Galpón", 64, "Campo", "bi bi-house", true, "Campo_Galpon", 2, 1 },
                    { 38, "Sistema de riego", 65, "Campo", "bi bi-droplet", true, "Campo_SistemaRiego", 2, 1 },
                    { 39, "Uso agrícola", 66, "Campo", "bi bi-seedling", true, "Campo_Agricola", 2, 1 },
                    { 40, "Uso ganadero", 67, "Campo", "bi bi-cow", true, "Campo_Ganadero", 2, 1 },
                    { 41, "Uso mixto", 68, "Campo", "bi bi-intersect", true, "Campo_Mixto", 2, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFeatures_FeatureDefinitionId",
                table: "PropertyFeatures",
                column: "FeatureDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyFeatures_FeatureDefinitions_FeatureDefinitionId",
                table: "PropertyFeatures",
                column: "FeatureDefinitionId",
                principalTable: "FeatureDefinitions",
                principalColumn: "FeatureDefinitionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyFeatures_FeatureDefinitions_FeatureDefinitionId",
                table: "PropertyFeatures");

            migrationBuilder.DropTable(
                name: "FeatureDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_PropertyFeatures_FeatureDefinitionId",
                table: "PropertyFeatures");

            migrationBuilder.RenameColumn(
                name: "FeatureDefinitionId",
                table: "PropertyFeatures",
                newName: "DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "PropertyFeatureId",
                table: "PropertyFeatures",
                newName: "FeatureId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PropertyFeatures",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
