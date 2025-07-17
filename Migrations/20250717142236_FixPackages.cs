using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class FixPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Packages_PackageOptionId",
                table: "Packages");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_PackageOptionId",
                table: "Packages",
                column: "PackageOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Packages_PackageOptionId",
                table: "Packages");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_PackageOptionId",
                table: "Packages",
                column: "PackageOptionId",
                unique: true);
        }
    }
}
