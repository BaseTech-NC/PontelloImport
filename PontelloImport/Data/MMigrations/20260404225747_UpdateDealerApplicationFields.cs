using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class UpdateDealerApplicationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealerApplications_Addresses_SubmittedAddressID",
                table: "DealerApplications");

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedEmail",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedContactPhone",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedContactName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedCompanyName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "SubmittedAddressID",
                table: "DealerApplications",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyDescription",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalZipCode",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceState",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteSocialMedia",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DealerApplications_Addresses_SubmittedAddressID",
                table: "DealerApplications",
                column: "SubmittedAddressID",
                principalTable: "Addresses",
                principalColumn: "AddressID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealerApplications_Addresses_SubmittedAddressID",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "City",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "CompanyDescription",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "PostalZipCode",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "ProvinceState",
                table: "DealerApplications");

            migrationBuilder.DropColumn(
                name: "WebsiteSocialMedia",
                table: "DealerApplications");

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedEmail",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedContactPhone",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedContactName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SubmittedCompanyName",
                table: "DealerApplications",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SubmittedAddressID",
                table: "DealerApplications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DealerApplications_Addresses_SubmittedAddressID",
                table: "DealerApplications",
                column: "SubmittedAddressID",
                principalTable: "Addresses",
                principalColumn: "AddressID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
