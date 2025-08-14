using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class CustomBP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CustomBP",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"),
                column: "CustomBP",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomBP",
                table: "Orders");
        }
    }
}
