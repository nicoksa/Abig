using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class deleteDniUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles",
                column: "Dni");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles",
                column: "Dni",
                unique: true,
                filter: "[Dni] IS NOT NULL");
        }
    }
}
