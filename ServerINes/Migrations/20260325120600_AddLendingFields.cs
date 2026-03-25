using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddLendingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Lendings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Direction",
                table: "Lendings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationSent",
                table: "Lendings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SendNotification",
                table: "Lendings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Lendings");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Lendings");

            migrationBuilder.DropColumn(
                name: "NotificationSent",
                table: "Lendings");

            migrationBuilder.DropColumn(
                name: "SendNotification",
                table: "Lendings");
        }
    }
}
