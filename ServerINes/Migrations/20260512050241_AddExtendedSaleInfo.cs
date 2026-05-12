using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INest.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedSaleInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Sales",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Sales",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformFee",
                table: "Sales",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePriceSnapshot",
                table: "Sales",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_CategoryId",
                table: "Sales",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Categories_CategoryId",
                table: "Sales",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Categories_CategoryId",
                table: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_Sales_CategoryId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PlatformFee",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "PurchasePriceSnapshot",
                table: "Sales");
        }
    }
}
