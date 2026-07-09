using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class AddIntlPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntlPackages",
                columns: table => new
                {
                    IntlPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: true),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    BoxNote = table.Column<string>(type: "text", nullable: false),
                    Address1 = table.Column<string>(type: "text", nullable: false),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntlPackages", x => x.IntlPackageId);
                    table.ForeignKey(
                        name: "FK_IntlPackages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "IntlPackages",
                columns: new[] { "IntlPackageId", "Address1", "Address2", "BoxNote", "City", "ContactName", "Country", "CustomerId", "Qty", "State", "ZipCode" },
                values: new object[] { new Guid("1c2e6b8a-4f5d-4a3b-9c1e-2d7f8a9b0c3d"), "123 Main", null, "Box 1 of 1", "Ripley", "Contact name", "Canada", 2777, 1, "NY", "14775" });

            migrationBuilder.CreateIndex(
                name: "IX_IntlPackages_CustomerId",
                table: "IntlPackages",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntlPackages");
        }
    }
}
