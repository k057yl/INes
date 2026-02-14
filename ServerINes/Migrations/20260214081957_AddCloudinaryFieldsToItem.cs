using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryFieldsToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Items");
        }
    }
}
