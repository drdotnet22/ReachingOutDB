using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    QtyQ1 = table.Column<int>(type: "int", nullable: false),
                    QtyQ2 = table.Column<int>(type: "int", nullable: false),
                    QtyQ3 = table.Column<int>(type: "int", nullable: false),
                    QtyQ4 = table.Column<int>(type: "int", nullable: false),
                    VariableQty = table.Column<bool>(type: "bit", nullable: false),
                    NotesQ1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesQ2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesQ3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesQ4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomBP = table.Column<bool>(type: "bit", nullable: false),
                    DmQty = table.Column<int>(type: "int", nullable: true),
                    DmQtyQ1 = table.Column<int>(type: "int", nullable: true),
                    DmQtyQ2 = table.Column<int>(type: "int", nullable: true),
                    DmQtyQ3 = table.Column<int>(type: "int", nullable: true),
                    DmQtyQ4 = table.Column<int>(type: "int", nullable: true),
                    UpsQty = table.Column<int>(type: "int", nullable: true),
                    UpsQtyQ1 = table.Column<int>(type: "int", nullable: true),
                    UpsQtyQ2 = table.Column<int>(type: "int", nullable: true),
                    UpsQtyQ3 = table.Column<int>(type: "int", nullable: true),
                    UpsQtyQ4 = table.Column<int>(type: "int", nullable: true),
                    PostalQty = table.Column<int>(type: "int", nullable: true),
                    PostalQtyQ1 = table.Column<int>(type: "int", nullable: true),
                    PostalQtyQ2 = table.Column<int>(type: "int", nullable: true),
                    PostalQtyQ3 = table.Column<int>(type: "int", nullable: true),
                    PostalQtyQ4 = table.Column<int>(type: "int", nullable: true),
                    LtlQty = table.Column<int>(type: "int", nullable: true),
                    LtlQtyQ1 = table.Column<int>(type: "int", nullable: true),
                    LtlQtyQ2 = table.Column<int>(type: "int", nullable: true),
                    LtlQtyQ3 = table.Column<int>(type: "int", nullable: true),
                    LtlQtyQ4 = table.Column<int>(type: "int", nullable: true),
                    IntlQty = table.Column<int>(type: "int", nullable: true),
                    IntlQtyQ1 = table.Column<int>(type: "int", nullable: true),
                    IntlQtyQ2 = table.Column<int>(type: "int", nullable: true),
                    IntlQtyQ3 = table.Column<int>(type: "int", nullable: true),
                    IntlQtyQ4 = table.Column<int>(type: "int", nullable: true),
                    YearlyBillingQuarter = table.Column<int>(type: "int", nullable: true),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "PackageOptions",
                columns: table => new
                {
                    PackageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackagingWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageOptions", x => x.PackageOptionId);
                });

            migrationBuilder.CreateTable(
                name: "ShippingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityPerBox = table.Column<int>(type: "int", nullable: false),
                    MarkupPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PerBoxFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BoxDiscountThreshold = table.Column<int>(type: "int", nullable: true),
                    BoxDiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    UserProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.UserProfileId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Quarter = table.Column<int>(type: "int", nullable: false),
                    JobStatus = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    SpecialNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotesForInvoicing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearlyBilling = table.Column<bool>(type: "bit", nullable: true),
                    HoldNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlateId = table.Column<int>(type: "int", nullable: true),
                    BpUpdate = table.Column<bool>(type: "bit", nullable: false),
                    DmQty = table.Column<int>(type: "int", nullable: true),
                    UpsQty = table.Column<int>(type: "int", nullable: true),
                    UpsCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PostalQty = table.Column<int>(type: "int", nullable: true),
                    PostalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IntlQty = table.Column<int>(type: "int", nullable: true),
                    IntlCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LtlQty = table.Column<int>(type: "int", nullable: true),
                    LTLCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PubUsps = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PubShipping = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PpOrderNumber = table.Column<int>(type: "int", nullable: true),
                    Archived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MailClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageId);
                    table.ForeignKey(
                        name: "FK_Packages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Packages_PackageOptions_PackageOptionId",
                        column: x => x.PackageOptionId,
                        principalTable: "PackageOptions",
                        principalColumn: "PackageOptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAuditLogs",
                columns: table => new
                {
                    OrderAuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAuditLogs", x => x.OrderAuditLogId);
                    table.ForeignKey(
                        name: "FK_OrderAuditLogs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "CustomBP", "CustomerName", "DmQty", "DmQtyQ1", "DmQtyQ2", "DmQtyQ3", "DmQtyQ4", "IntlQty", "IntlQtyQ1", "IntlQtyQ2", "IntlQtyQ3", "IntlQtyQ4", "Location", "LtlQty", "LtlQtyQ1", "LtlQtyQ2", "LtlQtyQ3", "LtlQtyQ4", "NotesQ1", "NotesQ2", "NotesQ3", "NotesQ4", "PackageId", "PostalQty", "PostalQtyQ1", "PostalQtyQ2", "PostalQtyQ3", "PostalQtyQ4", "QtyQ1", "QtyQ2", "QtyQ3", "QtyQ4", "UpsQty", "UpsQtyQ1", "UpsQtyQ2", "UpsQtyQ3", "UpsQtyQ4", "VariableQty", "YearlyBillingQuarter" },
                values: new object[] { 2000, true, false, "Mennonite Church", null, null, null, null, null, null, null, null, null, null, "PA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 0, 0, 0, 0, null, null, null, null, null, false, null });

            migrationBuilder.InsertData(
                table: "PackageOptions",
                columns: new[] { "PackageOptionId", "Height", "Length", "PackageDescription", "PackagingWeight", "Width" },
                values: new object[] { new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), null, null, "10x13 plastic sleeve", 0.1m, null });

            migrationBuilder.InsertData(
                table: "ShippingSettings",
                columns: new[] { "Id", "BoxDiscountPercentage", "BoxDiscountThreshold", "HandlingFee", "MarkupPercentage", "Name", "PerBoxFee", "QuantityPerBox", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0.15m, 4, 4m, 0.6m, "UPS", 1.75m, 750, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 0.15m, 4, 2m, 0.3m, "INTL", 1.25m, 750, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, 0.15m, 4, 25m, 0.15m, "LTL", 1.25m, 750, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, 0.15m, 4, 0m, 0.1m, "USPS", 1.5m, 200, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "UserProfiles",
                columns: new[] { "UserProfileId", "Active", "Name", "Role" },
                values: new object[] { 1, true, "Ryan Stauffer", 1 });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "Archived", "BpUpdate", "CustomerId", "DmQty", "HoldNote", "IntlCost", "IntlQty", "JobStatus", "LTLCost", "LtlQty", "NotesForInvoicing", "PlateId", "PostalCost", "PostalQty", "PpOrderNumber", "PubShipping", "PubUsps", "Qty", "Quarter", "SpecialNotes", "UpsCost", "UpsQty", "Year", "YearlyBilling" },
                values: new object[] { new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), false, false, 2000, null, null, null, null, 1, null, null, null, null, null, null, null, null, null, 0, 3, null, null, null, 2025, null });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "PackageId", "Address", "City", "ContactName", "CustomerId", "MailClass", "PackageOptionId", "State", "ZipCode" },
                values: new object[] { new Guid("07dd94e4-a0c8-43c8-babc-200a8864d02c"), "123 Main", "Ripley", "Contact name", 2000, "FCF", new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), "NY", "14775" });

            migrationBuilder.InsertData(
                table: "OrderAuditLogs",
                columns: new[] { "OrderAuditLogId", "Action", "NewValue", "OldValue", "OrderId", "PropertyName", "Timestamp", "UserName" },
                values: new object[] { new Guid("02bbd91b-1be0-4640-b82f-66b38ba448b9"), "Updated", "New Value", "Old Value", new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), "Some Property", new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Anonymous" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderAuditLogs_OrderId",
                table: "OrderAuditLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_CustomerId",
                table: "Packages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_PackageOptionId",
                table: "Packages",
                column: "PackageOptionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderAuditLogs");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "ShippingSettings");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "PackageOptions");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
