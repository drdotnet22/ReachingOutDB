using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReminderRules",
                columns: table => new
                {
                    ReminderRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RecipientEmails = table.Column<string>(type: "text", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    CheckIntervalMinutes = table.Column<int>(type: "integer", nullable: true),
                    IntervalValue = table.Column<int>(type: "integer", nullable: true),
                    IntervalUnit = table.Column<int>(type: "integer", nullable: true),
                    NextRunUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRunUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderRules", x => x.ReminderRuleId);
                });

            migrationBuilder.CreateTable(
                name: "SmtpSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    EnableSsl = table.Column<bool>(type: "boolean", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    FromAddress = table.Column<string>(type: "text", nullable: false),
                    FromName = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmtpSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderConditions",
                columns: table => new
                {
                    ReminderConditionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    Operator = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderConditions", x => x.ReminderConditionId);
                    table.ForeignKey(
                        name: "FK_ReminderConditions_ReminderRules_ReminderRuleId",
                        column: x => x.ReminderRuleId,
                        principalTable: "ReminderRules",
                        principalColumn: "ReminderRuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReminderLogs",
                columns: table => new
                {
                    ReminderLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderLogs", x => x.ReminderLogId);
                    table.ForeignKey(
                        name: "FK_ReminderLogs_ReminderRules_ReminderRuleId",
                        column: x => x.ReminderRuleId,
                        principalTable: "ReminderRules",
                        principalColumn: "ReminderRuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReminderConditions_ReminderRuleId",
                table: "ReminderConditions",
                column: "ReminderRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderLogs_ReminderRuleId_OrderId",
                table: "ReminderLogs",
                columns: new[] { "ReminderRuleId", "OrderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderConditions");

            migrationBuilder.DropTable(
                name: "ReminderLogs");

            migrationBuilder.DropTable(
                name: "SmtpSettings");

            migrationBuilder.DropTable(
                name: "ReminderRules");
        }
    }
}
