using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class P2_CompleteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Vendors_VendorID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Products_ProductID",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "ProductAttributes");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Handle",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_IsActive",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Price",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductID_IsActive",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Status",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Status_IsActive",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_Weight",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive",
                table: "Products");

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
                name: "Handle",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ProductVariants",
                newName: "Option5Value");

            migrationBuilder.RenameColumn(
                name: "Tags",
                table: "ProductVariants",
                newName: "VariantImageUrl");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "ProductVariants",
                newName: "IsDefault");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Vendors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Vendors",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Vendors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                table: "ProductVariants",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductVariants",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "ProductVariants",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option1Name",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option1Value",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option2Name",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option2Value",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option3Name",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option3Value",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option4Name",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option4Value",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option5Name",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeightUnit",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductTypeID",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "ProductCategories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductCategories",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ProductCategories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Street = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Province = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Country = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressID);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionTemplates",
                columns: table => new
                {
                    OptionTemplateID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PredefinedValues = table.Column<string>(type: "TEXT", nullable: true),
                    AllowCustomValue = table.Column<bool>(type: "INTEGER", nullable: false),
                    Suffix = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    HelpText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Placeholder = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionTemplates", x => x.OptionTemplateID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTerms",
                columns: table => new
                {
                    PaymentTermsID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TermName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TermCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TermDescription = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DaysUntilDue = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerms", x => x.PaymentTermsID);
                });

            migrationBuilder.CreateTable(
                name: "ProductSpecifications",
                columns: table => new
                {
                    SpecificationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSpecifications", x => x.SpecificationID);
                    table.ForeignKey(
                        name: "FK_ProductSpecifications_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    ProductTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TypeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TypeSlug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.ProductTypeID);
                });

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    AdminUserID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserID = table.Column<string>(type: "TEXT", nullable: false),
                    IsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.AdminUserID);
                    table.ForeignKey(
                        name: "FK_AdminUsers_ApplicationUser_ApplicationUserID",
                        column: x => x.ApplicationUserID,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    DealerID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationUserID = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BillingAddressID = table.Column<int>(type: "INTEGER", nullable: false),
                    ShippingAddressID = table.Column<int>(type: "INTEGER", nullable: true),
                    BusinessNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsTaxExempt = table.Column<bool>(type: "INTEGER", nullable: false),
                    PaymentTermsID = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.DealerID);
                    table.ForeignKey(
                        name: "FK_Dealers_Addresses_BillingAddressID",
                        column: x => x.BillingAddressID,
                        principalTable: "Addresses",
                        principalColumn: "AddressID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dealers_Addresses_ShippingAddressID",
                        column: x => x.ShippingAddressID,
                        principalTable: "Addresses",
                        principalColumn: "AddressID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dealers_ApplicationUser_ApplicationUserID",
                        column: x => x.ApplicationUserID,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Dealers_PaymentTerms_PaymentTermsID",
                        column: x => x.PaymentTermsID,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    CartID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DealerID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.CartID);
                    table.ForeignKey(
                        name: "FK_Carts_Dealers_DealerID",
                        column: x => x.DealerID,
                        principalTable: "Dealers",
                        principalColumn: "DealerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DealerApplications",
                columns: table => new
                {
                    ApplicationID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubmittedCompanyName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SubmittedContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SubmittedContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SubmittedEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SubmittedAddressID = table.Column<int>(type: "INTEGER", nullable: false),
                    BusinessNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    RequestedPaymentTermsID = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ReviewedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedDealerID = table.Column<int>(type: "INTEGER", nullable: true),
                    DealerID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DealerApplications", x => x.ApplicationID);
                    table.ForeignKey(
                        name: "FK_DealerApplications_Addresses_SubmittedAddressID",
                        column: x => x.SubmittedAddressID,
                        principalTable: "Addresses",
                        principalColumn: "AddressID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DealerApplications_Dealers_ApprovedDealerID",
                        column: x => x.ApprovedDealerID,
                        principalTable: "Dealers",
                        principalColumn: "DealerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealerApplications_Dealers_DealerID",
                        column: x => x.DealerID,
                        principalTable: "Dealers",
                        principalColumn: "DealerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DealerApplications_PaymentTerms_RequestedPaymentTermsID",
                        column: x => x.RequestedPaymentTermsID,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "INTEGER", nullable: false),
                    RootOrderID = table.Column<int>(type: "INTEGER", nullable: true),
                    DealerID = table.Column<int>(type: "INTEGER", nullable: false),
                    DealerCompanyName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TrackingNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsTaxExempt = table.Column<bool>(type: "INTEGER", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentTermsID = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PreviousOrderID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_Dealers_DealerID",
                        column: x => x.DealerID,
                        principalTable: "Dealers",
                        principalColumn: "DealerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Orders_PreviousOrderID",
                        column: x => x.PreviousOrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Orders_RootOrderID",
                        column: x => x.RootOrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_PaymentTerms_PaymentTermsID",
                        column: x => x.PaymentTermsID,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CartID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductVariantID = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    AddedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartID",
                        column: x => x.CartID,
                        principalTable: "Carts",
                        principalColumn: "CartID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductVariants_ProductVariantID",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "VariantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistories",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ChangeDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", nullable: true),
                    NewValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistories", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_OrderHistories_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                columns: table => new
                {
                    OrderLineID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductVariantID = table.Column<int>(type: "INTEGER", nullable: true),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    VariantTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => x.OrderLineID);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderLines_ProductVariants_ProductVariantID",
                        column: x => x.ProductVariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "VariantID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductTypeID",
                table: "Products",
                column: "ProductTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_ApplicationUserID",
                table: "AdminUsers",
                column: "ApplicationUserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartID",
                table: "CartItems",
                column: "CartID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantID",
                table: "CartItems",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_DealerID",
                table: "Carts",
                column: "DealerID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DealerApplications_ApprovedDealerID",
                table: "DealerApplications",
                column: "ApprovedDealerID");

            migrationBuilder.CreateIndex(
                name: "IX_DealerApplications_DealerID",
                table: "DealerApplications",
                column: "DealerID");

            migrationBuilder.CreateIndex(
                name: "IX_DealerApplications_RequestedPaymentTermsID",
                table: "DealerApplications",
                column: "RequestedPaymentTermsID");

            migrationBuilder.CreateIndex(
                name: "IX_DealerApplications_SubmittedAddressID",
                table: "DealerApplications",
                column: "SubmittedAddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_ApplicationUserID",
                table: "Dealers",
                column: "ApplicationUserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_BillingAddressID",
                table: "Dealers",
                column: "BillingAddressID");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_CompanyName",
                table: "Dealers",
                column: "CompanyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_PaymentTermsID",
                table: "Dealers",
                column: "PaymentTermsID");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_ShippingAddressID",
                table: "Dealers",
                column: "ShippingAddressID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistories_OrderID",
                table: "OrderHistories",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderID",
                table: "OrderLines",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_ProductVariantID",
                table: "OrderLines",
                column: "ProductVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DealerID",
                table: "Orders",
                column: "DealerID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentTermsID",
                table: "Orders",
                column: "PaymentTermsID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PreviousOrderID",
                table: "Orders",
                column: "PreviousOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RootOrderID",
                table: "Orders",
                column: "RootOrderID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_TermCode",
                table: "PaymentTerms",
                column: "TermCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTerms_TermName",
                table: "PaymentTerms",
                column: "TermName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductID",
                table: "ProductSpecifications",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryID",
                table: "Products",
                column: "ProductCategoryID",
                principalTable: "ProductCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeID",
                table: "Products",
                column: "ProductTypeID",
                principalTable: "ProductTypes",
                principalColumn: "ProductTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Vendors_VendorID",
                table: "Products",
                column: "VendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductID",
                table: "ProductVariants",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductTypes_ProductTypeID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Vendors_VendorID",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Products_ProductID",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "DealerApplications");

            migrationBuilder.DropTable(
                name: "OptionTemplates");

            migrationBuilder.DropTable(
                name: "OrderHistories");

            migrationBuilder.DropTable(
                name: "OrderLines");

            migrationBuilder.DropTable(
                name: "ProductSpecifications");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductTypeID",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option1Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option1Value",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option2Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option2Value",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option3Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option3Value",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option4Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option4Value",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Option5Name",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "WeightUnit",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ProductTypeID",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "VariantImageUrl",
                table: "ProductVariants",
                newName: "Tags");

            migrationBuilder.RenameColumn(
                name: "Option5Value",
                table: "ProductVariants",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                table: "ProductVariants",
                newName: "IsActive");

            migrationBuilder.AlterColumn<int>(
                name: "ModifiedBy",
                table: "Vendors",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Vendors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Vendors",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Weight",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "ModifiedBy",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "ProductVariants",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductVariants",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Handle",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "ProductVariants",
                type: "TEXT",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AlterColumn<int>(
                name: "ModifiedBy",
                table: "Products",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Products",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Products",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ModifiedBy",
                table: "ProductCategories",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "ProductCategories",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "ProductCategories",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ProductAttributes",
                columns: table => new
                {
                    AttributeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VariantID = table.Column<int>(type: "INTEGER", nullable: false),
                    AttributeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AttributeValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsVariantAttribute = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModifiedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAttributes", x => x.AttributeID);
                    table.ForeignKey(
                        name: "FK_ProductAttributes_ProductVariants_VariantID",
                        column: x => x.VariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "VariantID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Handle",
                table: "ProductVariants",
                column: "Handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_IsActive",
                table: "ProductVariants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Price",
                table: "ProductVariants",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductID_IsActive",
                table: "ProductVariants",
                columns: new[] { "ProductID", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Status",
                table: "ProductVariants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Status_IsActive",
                table: "ProductVariants",
                columns: new[] { "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_Weight",
                table: "ProductVariants",
                column: "Weight");

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive",
                table: "Products",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Status_IsActive",
                table: "Products",
                columns: new[] { "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeName",
                table: "ProductAttributes",
                column: "AttributeName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_AttributeName_AttributeValue",
                table: "ProductAttributes",
                columns: new[] { "AttributeName", "AttributeValue" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_VariantID",
                table: "ProductAttributes",
                column: "VariantID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAttributes_VariantID_IsVariantAttribute",
                table: "ProductAttributes",
                columns: new[] { "VariantID", "IsVariantAttribute" });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_ProductCategoryID",
                table: "Products",
                column: "ProductCategoryID",
                principalTable: "ProductCategories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Vendors_VendorID",
                table: "Products",
                column: "VendorID",
                principalTable: "Vendors",
                principalColumn: "VendorID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductID",
                table: "ProductVariants",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
