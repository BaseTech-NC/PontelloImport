using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class AddAddressIsPrimary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DealerID",
                table: "Addresses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Addresses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DealerID",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Addresses");
        }
    }
}
