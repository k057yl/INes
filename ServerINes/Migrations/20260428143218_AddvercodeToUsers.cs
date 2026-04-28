using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddvercodeToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationCodeExpiryTime",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationCodeExpiryTime",
                table: "AspNetUsers");
        }
    }
}
