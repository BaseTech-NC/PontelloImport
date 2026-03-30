using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddStockPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderSequence",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "StockPolicy",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "deny");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockPolicy",
                table: "ProductVariants");

            migrationBuilder.InsertData(
                table: "OrderSequence",
                columns: new[] { "Id", "LastUsedNumber" },
                values: new object[] { 1, 0 });
        }
    }
}
