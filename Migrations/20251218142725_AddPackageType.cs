using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackageType",
                table: "Packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "PackageId",
                keyValue: new Guid("07dd94e4-a0c8-43c8-babc-200a8864d02c"),
                column: "PackageType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "Packages");
        }
    }
}
