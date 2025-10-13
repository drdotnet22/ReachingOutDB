using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReachingOutDB.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyCustomerQtyAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "QtyQ4",
                table: "Customers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "QtyQ3",
                table: "Customers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "QtyQ2",
                table: "Customers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "QtyQ1",
                table: "Customers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.RenameColumn(
                name: "VariableQty",
                table: "Customers",
                newName: "VariableOrders");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Qty",
                table: "Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2000,
                columns: new[] { "Notes", "Qty" },
                values: new object[] { null, 0 });

            migrationBuilder.Sql(@"
                UPDATE ""Customers""
                SET ""Qty"" = COALESCE(""QtyQ1"", 0),
                    ""QtyQ1"" = NULL,
                    ""QtyQ2"" = NULL,
                    ""QtyQ3"" = NULL,
                    ""QtyQ4"" = NULL
                WHERE ""VariableOrders"" = false
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Customers""
                SET ""Notes"" = COALESCE(""NotesQ1"", NULL),
                    ""NotesQ1"" = NULL,
                    ""NotesQ2"" = NULL,
                    ""NotesQ3"" = NULL,
                    ""NotesQ4"" = NULL
                WHERE ""VariableOrders"" = false
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Qty",
                table: "Customers");
        }
    }
}
