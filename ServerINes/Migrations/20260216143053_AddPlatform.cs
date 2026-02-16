using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_StorageLocations_PlatformId",
                table: "Sales");

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Platforms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_UserId",
                table: "Platforms",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Platforms_PlatformId",
                table: "Sales",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Platforms_PlatformId",
                table: "Sales");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_StorageLocations_PlatformId",
                table: "Sales",
                column: "PlatformId",
                principalTable: "StorageLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
