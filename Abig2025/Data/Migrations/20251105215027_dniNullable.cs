using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class dniNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "UserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles",
                column: "Dni",
                unique: true,
                filter: "[Dni] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "UserProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Dni",
                table: "UserProfiles",
                column: "Dni",
                unique: true);
        }
    }
}
