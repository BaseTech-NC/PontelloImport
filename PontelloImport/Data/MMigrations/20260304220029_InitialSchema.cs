using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontelloImport.Data.PIMigrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "OrderSequence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastUsedNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSequence", x => x.Id);
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
                name: "ProductCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CategorySlug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CategoryDescription = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentCategoryID = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_ProductCategories_ProductCategories_ParentCategoryID",
                        column: x => x.ParentCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Vendors",
                columns: table => new
                {
                    VendorID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VendorName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    VendorSlug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Website = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.VendorID);
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
                        name: "FK_Dealers_PaymentTerms_PaymentTermsID",
                        column: x => x.PaymentTermsID,
                        principalTable: "PaymentTerms",
                        principalColumn: "PaymentTermsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Handle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VendorID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductCategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductTypeID = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_ProductCategoryID",
                        column: x => x.ProductCategoryID,
                        principalTable: "ProductCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_ProductTypes_ProductTypeID",
                        column: x => x.ProductTypeID,
                        principalTable: "ProductTypes",
                        principalColumn: "ProductTypeID");
                    table.ForeignKey(
                        name: "FK_Products_Vendors_VendorID",
                        column: x => x.VendorID,
                        principalTable: "Vendors",
                        principalColumn: "VendorID",
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
                name: "ProductVariants",
                columns: table => new
                {
                    VariantID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductID = table.Column<int>(type: "INTEGER", nullable: false),
                    SKU = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CompareAtPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CostPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    InventoryQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    InventoryPolicy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    WeightUnit = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    RequiresShipping = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsTaxable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Option1Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option1Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option2Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option2Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option3Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option3Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option4Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option4Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option5Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Option5Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    VariantImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.VariantID);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
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

            migrationBuilder.InsertData(
                table: "OrderSequence",
                columns: new[] { "Id", "LastUsedNumber" },
                values: new object[] { 1, 0 });

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
                name: "IX_ProductCategories_CategoryName",
                table: "ProductCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategorySlug",
                table: "ProductCategories",
                column: "CategorySlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_ParentCategoryID",
                table: "ProductCategories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Handle",
                table: "Products",
                column: "Handle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCategoryID",
                table: "Products",
                column: "ProductCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductTypeID",
                table: "Products",
                column: "ProductTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VendorID",
                table: "Products",
                column: "VendorID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSpecifications_ProductID",
                table: "ProductSpecifications",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductID",
                table: "ProductVariants",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants",
                column: "SKU",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorName",
                table: "Vendors",
                column: "VendorName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorSlug",
                table: "Vendors",
                column: "VendorSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "OrderSequence");

            migrationBuilder.DropTable(
                name: "ProductSpecifications");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "PaymentTerms");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
