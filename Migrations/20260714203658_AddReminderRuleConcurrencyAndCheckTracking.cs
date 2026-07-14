using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderRuleConcurrencyAndCheckTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCheckedUtc",
                table: "ReminderRules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ReminderRules",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCheckedUtc",
                table: "ReminderRules");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ReminderRules");
        }
    }
}
