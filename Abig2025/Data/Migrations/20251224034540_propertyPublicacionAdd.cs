using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Abig2025.Data.Migrations
{
    /// <inheritdoc />
    public partial class propertyPublicacionAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyPublications",
                columns: table => new
                {
                    PublicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPublications", x => x.PublicationId);
                    table.ForeignKey(
                        name: "FK_PropertyPublications_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "PropertyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyPublications_SubscriptionPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "SubscriptionPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PropertyPublications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_ExpiresAt",
                table: "PropertyPublications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_IsActive",
                table: "PropertyPublications",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PlanId",
                table: "PropertyPublications",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PropertyId_UserId_PlanId_PublishedAt",
                table: "PropertyPublications",
                columns: new[] { "PropertyId", "UserId", "PlanId", "PublishedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_PublishedAt",
                table: "PropertyPublications",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyPublications_UserId",
                table: "PropertyPublications",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyPublications");
        }
    }
}
