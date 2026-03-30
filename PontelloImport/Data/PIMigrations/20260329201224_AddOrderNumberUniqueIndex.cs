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
            // Drop existing non-unique index (Azure DB already has it from InitialSchema).
            // Using raw SQL so IF EXISTS prevents failure if already dropped.
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Orders_OrderNumber\";");

            // Reassign ALL orders sequentially by OrderID — guarantees uniqueness
            // because OrderID is the PK. No WHERE clause needed.
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET OrderNumber = substr('0000' || CAST(OrderID AS TEXT), -4, 4)
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
