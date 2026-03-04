using PontelloImport.Models;

namespace PontelloImport.Data
{
    public static class PontelloDbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<PontelloDbContext>();

                if (context == null) return;

                // Schema is managed by EF migrations (Migrate() called in Program.cs)

                // Guard: already seeded
                if (context.Vendors.Any()) return;

                // ===== SEED 1 — PAYMENT TERMS =====

                var paymentTerms = new List<PaymentTerms>
                {
                    new PaymentTerms
                    {
                        TermName        = "Cash on Delivery",
                        TermCode        = "COD",
                        TermDescription = "Payment due before delivery",
                        DaysUntilDue    = 0,
                        DisplayOrder    = 1,
                        IsActive        = true
                    },
                    new PaymentTerms
                    {
                        TermName        = "Net 30",
                        TermCode        = "NET30",
                        TermDescription = "Payment due 30 days after order",
                        DaysUntilDue    = 30,
                        DisplayOrder    = 2,
                        IsActive        = true
                    },
                    new PaymentTerms
                    {
                        TermName        = "Net 60",
                        TermCode        = "NET60",
                        TermDescription = "Payment due 60 days after order",
                        DaysUntilDue    = 60,
                        DisplayOrder    = 3,
                        IsActive        = true
                    },
                    new PaymentTerms
                    {
                        TermName        = "Net 90",
                        TermCode        = "NET90",
                        TermDescription = "Payment due 90 days after order",
                        DaysUntilDue    = 90,
                        DisplayOrder    = 4,
                        IsActive        = true
                    }
                };

                context.PaymentTerms.AddRange(paymentTerms);
                context.SaveChanges();

                // ===== SEED 2 — VENDORS =====

                var vendors = new List<Vendor>
                {
                    new Vendor { VendorName = "Birel ART",      VendorSlug = "birel-art",      Country = "Italy", IsActive = true },
                    new Vendor { VendorName = "Vortex Engines", VendorSlug = "vortex-engines", Country = "Italy", IsActive = true },
                    new Vendor { VendorName = "MG Tires",       VendorSlug = "mg-tires",       Country = "Italy", IsActive = true },
                    new Vendor { VendorName = "CRG",            VendorSlug = "crg",            Country = "Italy", IsActive = true },
                    new Vendor { VendorName = "Tony Kart",      VendorSlug = "tony-kart",      Country = "Italy", IsActive = true }
                };

                context.Vendors.AddRange(vendors);
                context.SaveChanges();

                // Named references for use in Products below
                var vendorVortex   = vendors.First(v => v.VendorSlug == "vortex-engines");
                var vendorMgTires  = vendors.First(v => v.VendorSlug == "mg-tires");
                var vendorCrg      = vendors.First(v => v.VendorSlug == "crg");

                // ===== SEED 3 — PRODUCT CATEGORIES =====

                var categories = new List<ProductCategory>
                {
                    new ProductCategory { CategoryName = "Chassis",          CategorySlug = "chassis",          DisplayOrder = 1, IsActive = true },
                    new ProductCategory { CategoryName = "Tires",            CategorySlug = "tires",            DisplayOrder = 2, IsActive = true },
                    new ProductCategory { CategoryName = "Engines",          CategorySlug = "engines",          DisplayOrder = 3, IsActive = true },
                    new ProductCategory { CategoryName = "Accessories",      CategorySlug = "accessories",      DisplayOrder = 4, IsActive = true },
                    new ProductCategory { CategoryName = "Safety Equipment", CategorySlug = "safety-equipment", DisplayOrder = 5, IsActive = true },
                    new ProductCategory { CategoryName = "Tools",            CategorySlug = "tools",            DisplayOrder = 6, IsActive = true }
                };

                context.ProductCategories.AddRange(categories);
                context.SaveChanges();

                var catTires       = categories.First(c => c.CategorySlug == "tires");
                var catEngines     = categories.First(c => c.CategorySlug == "engines");
                var catAccessories = categories.First(c => c.CategorySlug == "accessories");

                // ===== SEED 4 — PRODUCT TYPES =====

                var productTypes = new List<ProductType>
                {
                    new ProductType { TypeName = "Tire",   TypeSlug = "tire",   DisplayOrder = 1, IsActive = true },
                    new ProductType { TypeName = "Engine", TypeSlug = "engine", DisplayOrder = 2, IsActive = true },
                    new ProductType { TypeName = "Frame",  TypeSlug = "frame",  DisplayOrder = 3, IsActive = true },
                    new ProductType { TypeName = "Part",   TypeSlug = "part",   DisplayOrder = 4, IsActive = true }
                };

                context.ProductTypes.AddRange(productTypes);
                context.SaveChanges();

                // ===== SEED 5 — PRODUCTS + VARIANTS =====

                // --- Product 1: Nerf Bar (standalone, single default variant) ---

                var nerfBar = new Product
                {
                    Title             = "Nerf Bar",
                    Handle            = "nerf-bar",
                    VendorID          = vendorCrg.VendorID,
                    ProductCategoryID = catAccessories.CategoryID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(nerfBar);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = nerfBar.ProductID,
                    SKU               = "CRG-NB-001",
                    Price             = 45.99m,
                    InventoryQuantity = 20,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 2: MG Tire Yellow (multi-variant, 1 option dimension) ---

                var mgTireYellow = new Product
                {
                    Title             = "MG Tire Yellow",
                    Handle            = "mg-tire-yellow",
                    VendorID          = vendorMgTires.VendorID,
                    ProductCategoryID = catTires.CategoryID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(mgTireYellow);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = mgTireYellow.ProductID,
                        SKU               = "MG-TY-S",
                        Price             = 89.99m,
                        InventoryQuantity = 15,
                        IsDefault         = false,
                        Option1Name       = "Compound",
                        Option1Value      = "Soft",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = mgTireYellow.ProductID,
                        SKU               = "MG-TY-M",
                        Price             = 89.99m,
                        InventoryQuantity = 10,
                        IsDefault         = false,
                        Option1Name       = "Compound",
                        Option1Value      = "Medium",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // --- Product 3: Vortex ROK GP Engine (draft) ---

                var vortexRok = new Product
                {
                    Title             = "Vortex ROK GP Engine",
                    Handle            = "vortex-rok-gp",
                    VendorID          = vendorVortex.VendorID,
                    ProductCategoryID = catEngines.CategoryID,
                    Status            = ProductStatus.Draft
                };

                context.Products.Add(vortexRok);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = vortexRok.ProductID,
                    SKU               = "VTX-ROK-GP-001",
                    Price             = 1299.99m,
                    InventoryQuantity = 3,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Draft
                });

                context.SaveChanges();
            }
        }
    }
}
