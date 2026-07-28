using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderAuditLogs",
                keyColumn: "OrderAuditLogId",
                keyValue: new Guid("02bbd91b-1be0-4640-b82f-66b38ba448b9"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "PackageId",
                keyValue: new Guid("07dd94e4-a0c8-43c8-babc-200a8864d02c"));

            migrationBuilder.DeleteData(
                table: "PlateAssignments",
                keyColumn: "PlateAssignmentId",
                keyValue: new Guid("5ad5f77b-d3cc-4454-acbe-a5cbbc7f158e"));

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "OrderId",
                keyValue: new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"));

            migrationBuilder.DeleteData(
                table: "PackageOptions",
                keyColumn: "PackageOptionId",
                keyValue: new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"));

            migrationBuilder.DeleteData(
                table: "Plates",
                keyColumn: "PlateId",
                keyValue: new Guid("6447999c-271d-4985-6275-08ddc619be12"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Active", "BackpageUpdates", "CustomBP", "CustomerName", "DmQty", "DmQtyQ1", "DmQtyQ2", "DmQtyQ3", "DmQtyQ4", "IntlQty", "IntlQtyQ1", "IntlQtyQ2", "IntlQtyQ3", "IntlQtyQ4", "Location", "LtlQty", "LtlQtyQ1", "LtlQtyQ2", "LtlQtyQ3", "LtlQtyQ4", "MailingNotes", "Notes", "NotesQ1", "NotesQ2", "NotesQ3", "NotesQ4", "PackageId", "PostalQty", "PostalQtyQ1", "PostalQtyQ2", "PostalQtyQ3", "PostalQtyQ4", "Qty", "QtyQ1", "QtyQ2", "QtyQ3", "QtyQ4", "SpecialNoteUPS", "UpsQty", "UpsQtyQ1", "UpsQtyQ2", "UpsQtyQ3", "UpsQtyQ4", "VariableOrders", "YearlyBillingQuarter" },
                values: new object[] { 2000, true, false, false, "Mennonite Church", null, null, null, null, null, null, null, null, null, null, "PA", null, null, null, null, null, "test", null, null, null, null, null, null, null, null, null, null, null, null, 0, 0, 0, 0, false, null, null, null, null, null, false, null });

            migrationBuilder.InsertData(
                table: "PackageOptions",
                columns: new[] { "PackageOptionId", "Height", "Length", "PackageDescription", "PackagingWeight", "Width" },
                values: new object[] { new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), null, null, "10x13 plastic sleeve", 0.1m, null });

            migrationBuilder.InsertData(
                table: "Plates",
                columns: new[] { "PlateId", "HasBlanks", "IsPlated", "Number", "Quantity", "Quarter", "Year" },
                values: new object[] { new Guid("6447999c-271d-4985-6275-08ddc619be12"), false, false, 1, 1, 1, 1 });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "OrderId", "Archived", "BpUpdate", "CustomBP", "CustomerId", "DmCost", "DmQty", "HoldNote", "IntlCost", "IntlQty", "JobStatus", "LTLCost", "LtlQty", "NotesForInvoicing", "PlateId", "PostalCost", "PostalQty", "PpOrderNumber", "PubShipping", "PubUsps", "Qty", "Quarter", "SpecialNotes", "UpsCost", "UpsQty", "Year", "YearlyBilling" },
                values: new object[] { new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), false, false, false, 2000, null, null, null, null, null, 1, null, null, null, null, null, null, null, null, null, 0, 3, null, null, null, 2025, null });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "PackageId", "Address", "City", "ContactName", "CustomerId", "MailClass", "PackageOptionId", "PackageType", "Qty", "State", "ZipCode" },
                values: new object[] { new Guid("07dd94e4-a0c8-43c8-babc-200a8864d02c"), "123 Main", "Ripley", "Contact name", 2000, "FCF", new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485c"), 0, 1, "NY", "14775" });

            migrationBuilder.InsertData(
                table: "OrderAuditLogs",
                columns: new[] { "OrderAuditLogId", "Action", "NewValue", "OldValue", "OrderId", "PropertyName", "Timestamp", "UserName" },
                values: new object[] { new Guid("02bbd91b-1be0-4640-b82f-66b38ba448b9"), "Updated", "New Value", "Old Value", new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), "Some Property", new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Anonymous" });

            migrationBuilder.InsertData(
                table: "PlateAssignments",
                columns: new[] { "PlateAssignmentId", "IsBlank", "OrderId", "PlateId", "Position" },
                values: new object[] { new Guid("5ad5f77b-d3cc-4454-acbe-a5cbbc7f158e"), false, new Guid("9b19ad13-0c8c-43dc-8c7d-9d4f3e1e485d"), new Guid("6447999c-271d-4985-6275-08ddc619be12"), 1 });
        }
    }
}
