using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddProductStatusAndVariantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BodyHTML",
                table: "Products",
                newName: "Description");

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VendorID",
                table: "Products",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "ProductCategoryID",
                table: "Products",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Status",
                table: "ProductVariants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Status_IsActive",
                table: "ProductVariants",
                columns: new[] { "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status_IsActive",
                table: "Products",
                columns: new[] { "Status", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Status",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Status_IsActive",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_Products_Status",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Status_IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "BodyHTML");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "ProductVariants",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VendorID",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductCategoryID",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
