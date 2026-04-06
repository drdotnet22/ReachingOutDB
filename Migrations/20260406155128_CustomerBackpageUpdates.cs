using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class CustomerBackpageUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BackpageUpdates",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2000,
                column: "BackpageUpdates",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackpageUpdates",
                table: "Customers");
        }
    }
}
