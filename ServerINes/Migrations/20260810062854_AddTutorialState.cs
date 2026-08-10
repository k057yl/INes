using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorialState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedTutorials",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedTutorials",
                table: "AspNetUsers");
        }
    }
}
