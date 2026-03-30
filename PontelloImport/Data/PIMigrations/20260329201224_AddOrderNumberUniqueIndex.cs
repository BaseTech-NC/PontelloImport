using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddOrderNumberUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders");

            // Deduplicate OrderNumbers before enforcing uniqueness.
            // Assigns sequential numbers (based on OrderID) to any rows
            // that share an OrderNumber with another row.
            migrationBuilder.Sql(@"
                UPDATE Orders SET OrderNumber = printf('%04d', OrderID)
                WHERE OrderNumber IN (
                    SELECT OrderNumber FROM Orders
                    GROUP BY OrderNumber
                    HAVING COUNT(*) > 1
                )
            ");

            // Sync OrderSequence to the highest assigned number.
            migrationBuilder.Sql(@"
                UPDATE OrderSequence
                SET LastUsedNumber = (SELECT MAX(CAST(OrderNumber AS INTEGER)) FROM Orders WHERE OrderNumber GLOB '[0-9]*')
                WHERE Id = 1
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber");
        }
    }
}
