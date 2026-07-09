using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyConstraintWithIntlPackageSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IntlPackages",
                keyColumn: "IntlPackageId",
                keyValue: new Guid("1c2e6b8a-4f5d-4a3b-9c1e-2d7f8a9b0c3d"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IntlPackages",
                columns: new[] { "IntlPackageId", "Address1", "Address2", "BoxNote", "City", "ContactName", "Country", "CustomerId", "Qty", "State", "ZipCode" },
                values: new object[] { new Guid("1c2e6b8a-4f5d-4a3b-9c1e-2d7f8a9b0c3d"), "123 Main", null, "Box 1 of 1", "Ripley", "Contact name", "Canada", 2000, 1, "NY", "14775" });
        }
    }
}
