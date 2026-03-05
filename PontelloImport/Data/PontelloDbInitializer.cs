using Microsoft.EntityFrameworkCore;
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

                // ===== SEED 2 — VENDORS (12 total) =====

                var vendors = new List<Vendor>
                {
                    new Vendor { VendorName = "Birel ART",            VendorSlug = "birel-art",            Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "Vortex Engines",       VendorSlug = "vortex-engines",       Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "MG Tires",             VendorSlug = "mg-tires",             Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "CRG",                  VendorSlug = "crg",                  Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "Tony Kart",            VendorSlug = "tony-kart",            Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "OTK Kart Group",       VendorSlug = "otk-kart-group",       Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "Rotax",                VendorSlug = "rotax",                Country = "Austria",        IsActive = true },
                    new Vendor { VendorName = "Bridgestone Motorsport",VendorSlug = "bridgestone-motorsport",Country = "Japan",         IsActive = true },
                    new Vendor { VendorName = "Alfano",               VendorSlug = "alfano",               Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "Tillett Racing Seats", VendorSlug = "tillett-racing-seats", Country = "United Kingdom", IsActive = true },
                    new Vendor { VendorName = "Sparco",               VendorSlug = "sparco",               Country = "Italy",          IsActive = true },
                    new Vendor { VendorName = "Mychron",              VendorSlug = "mychron",              Country = "Italy",          IsActive = true }
                };

                context.Vendors.AddRange(vendors);
                context.SaveChanges();

                // Named references
                var vendorVortex      = vendors.First(v => v.VendorSlug == "vortex-engines");
                var vendorMgTires     = vendors.First(v => v.VendorSlug == "mg-tires");
                var vendorCrg         = vendors.First(v => v.VendorSlug == "crg");
                var vendorBirelArt    = vendors.First(v => v.VendorSlug == "birel-art");
                var vendorTonyKart    = vendors.First(v => v.VendorSlug == "tony-kart");
                var vendorOtk         = vendors.First(v => v.VendorSlug == "otk-kart-group");
                var vendorRotax       = vendors.First(v => v.VendorSlug == "rotax");
                var vendorBridgestone = vendors.First(v => v.VendorSlug == "bridgestone-motorsport");
                var vendorAlfano      = vendors.First(v => v.VendorSlug == "alfano");
                var vendorTillett     = vendors.First(v => v.VendorSlug == "tillett-racing-seats");
                var vendorSparco      = vendors.First(v => v.VendorSlug == "sparco");
                var vendorMychron     = vendors.First(v => v.VendorSlug == "mychron");

                // ===== SEED 3 — PRODUCT CATEGORIES (10 total) =====

                var categories = new List<ProductCategory>
                {
                    new ProductCategory { CategoryName = "Chassis",          CategorySlug = "chassis",          DisplayOrder = 1,  IsActive = true },
                    new ProductCategory { CategoryName = "Tires",            CategorySlug = "tires",            DisplayOrder = 2,  IsActive = true },
                    new ProductCategory { CategoryName = "Engines",          CategorySlug = "engines",          DisplayOrder = 3,  IsActive = true },
                    new ProductCategory { CategoryName = "Accessories",      CategorySlug = "accessories",      DisplayOrder = 4,  IsActive = true },
                    new ProductCategory { CategoryName = "Safety Equipment", CategorySlug = "safety-equipment", DisplayOrder = 5,  IsActive = true },
                    new ProductCategory { CategoryName = "Tools",            CategorySlug = "tools",            DisplayOrder = 6,  IsActive = true },
                    new ProductCategory { CategoryName = "Brakes",           CategorySlug = "brakes",           DisplayOrder = 7,  IsActive = true },
                    new ProductCategory { CategoryName = "Bodywork",         CategorySlug = "bodywork",         DisplayOrder = 8,  IsActive = true },
                    new ProductCategory { CategoryName = "Electronics",      CategorySlug = "electronics",      DisplayOrder = 9,  IsActive = true },
                    new ProductCategory { CategoryName = "Lubricants",       CategorySlug = "lubricants",       DisplayOrder = 10, IsActive = true }
                };

                context.ProductCategories.AddRange(categories);
                context.SaveChanges();

                var catTires       = categories.First(c => c.CategorySlug == "tires");
                var catEngines     = categories.First(c => c.CategorySlug == "engines");
                var catAccessories = categories.First(c => c.CategorySlug == "accessories");
                var catChassis     = categories.First(c => c.CategorySlug == "chassis");
                var catSafety      = categories.First(c => c.CategorySlug == "safety-equipment");
                var catBrakes      = categories.First(c => c.CategorySlug == "brakes");
                var catBodywork    = categories.First(c => c.CategorySlug == "bodywork");
                var catElectronics = categories.First(c => c.CategorySlug == "electronics");
                var catLubricants  = categories.First(c => c.CategorySlug == "lubricants");

                // ===== SEED 4 — PRODUCT TYPES (8 total) =====

                var productTypes = new List<ProductType>
                {
                    new ProductType { TypeName = "Tire",        TypeSlug = "tire",        DisplayOrder = 1, IsActive = true },
                    new ProductType { TypeName = "Engine",      TypeSlug = "engine",      DisplayOrder = 2, IsActive = true },
                    new ProductType { TypeName = "Frame",       TypeSlug = "frame",       DisplayOrder = 3, IsActive = true },
                    new ProductType { TypeName = "Part",        TypeSlug = "part",        DisplayOrder = 4, IsActive = true },
                    new ProductType { TypeName = "Seat",        TypeSlug = "seat",        DisplayOrder = 5, IsActive = true },
                    new ProductType { TypeName = "Brake",       TypeSlug = "brake",       DisplayOrder = 6, IsActive = true },
                    new ProductType { TypeName = "Bodywork",    TypeSlug = "bodywork",    DisplayOrder = 7, IsActive = true },
                    new ProductType { TypeName = "Electronics", TypeSlug = "electronics", DisplayOrder = 8, IsActive = true }
                };

                context.ProductTypes.AddRange(productTypes);
                context.SaveChanges();

                var typeTire        = productTypes.First(t => t.TypeSlug == "tire");
                var typeEngine      = productTypes.First(t => t.TypeSlug == "engine");
                var typeFrame       = productTypes.First(t => t.TypeSlug == "frame");
                var typePart        = productTypes.First(t => t.TypeSlug == "part");
                var typeSeat        = productTypes.First(t => t.TypeSlug == "seat");
                var typeBrake       = productTypes.First(t => t.TypeSlug == "brake");
                var typeBodywork    = productTypes.First(t => t.TypeSlug == "bodywork");
                var typeElectronics = productTypes.First(t => t.TypeSlug == "electronics");

                // ===== SEED 5 — PRODUCTS + VARIANTS (20 total) =====

                // --- Product 1: Nerf Bar (standalone) ---

                var nerfBar = new Product
                {
                    Title             = "Nerf Bar",
                    Handle            = "nerf-bar",
                    VendorID          = vendorCrg.VendorID,
                    ProductCategoryID = catAccessories.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
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

                // --- Product 2: MG Tire Yellow (multi-variant: Compound) ---

                var mgTireYellow = new Product
                {
                    Title             = "MG Tire Yellow",
                    Handle            = "mg-tire-yellow",
                    VendorID          = vendorMgTires.VendorID,
                    ProductCategoryID = catTires.CategoryID,
                    ProductTypeID     = typeTire.ProductTypeID,
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
                    ProductTypeID     = typeEngine.ProductTypeID,
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

                // --- Product 4: Racing Gloves (standalone) ---

                var racingGloves = new Product
                {
                    Title             = "Racing Gloves",
                    Handle            = "racing-gloves",
                    VendorID          = vendorSparco.VendorID,
                    ProductCategoryID = catSafety.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(racingGloves);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = racingGloves.ProductID,
                    SKU               = "SEED-RG-001",
                    Price             = 49.99m,
                    InventoryQuantity = 50,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 5: Kart Helmet (standalone) ---

                var kartHelmet = new Product
                {
                    Title             = "Kart Helmet",
                    Handle            = "kart-helmet",
                    VendorID          = vendorSparco.VendorID,
                    ProductCategoryID = catSafety.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(kartHelmet);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = kartHelmet.ProductID,
                    SKU               = "SEED-KH-001",
                    Price             = 199.99m,
                    InventoryQuantity = 25,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 6: Brake Disc 206mm (standalone) ---

                var brakeDisc = new Product
                {
                    Title             = "Brake Disc 206mm",
                    Handle            = "brake-disc-206mm",
                    VendorID          = vendorBirelArt.VendorID,
                    ProductCategoryID = catBrakes.CategoryID,
                    ProductTypeID     = typeBrake.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(brakeDisc);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = brakeDisc.ProductID,
                    SKU               = "SEED-BD-001",
                    Price             = 34.99m,
                    InventoryQuantity = 40,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 7: Brake Caliper Kit (standalone) ---

                var brakeCaliperKit = new Product
                {
                    Title             = "Brake Caliper Kit",
                    Handle            = "brake-caliper-kit",
                    VendorID          = vendorCrg.VendorID,
                    ProductCategoryID = catBrakes.CategoryID,
                    ProductTypeID     = typeBrake.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(brakeCaliperKit);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = brakeCaliperKit.ProductID,
                    SKU               = "SEED-BCK-001",
                    Price             = 89.99m,
                    InventoryQuantity = 20,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 8: Front Fairing (2 variants: Color Black/White) ---

                var frontFairing = new Product
                {
                    Title             = "Front Fairing",
                    Handle            = "front-fairing",
                    VendorID          = vendorOtk.VendorID,
                    ProductCategoryID = catBodywork.CategoryID,
                    ProductTypeID     = typeBodywork.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(frontFairing);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = frontFairing.ProductID,
                        SKU               = "SEED-FF-BLK",
                        Price             = 75.99m,
                        InventoryQuantity = 15,
                        IsDefault         = true,
                        Option1Name       = "Color",
                        Option1Value      = "Black",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = frontFairing.ProductID,
                        SKU               = "SEED-FF-WHT",
                        Price             = 75.99m,
                        InventoryQuantity = 15,
                        IsDefault         = false,
                        Option1Name       = "Color",
                        Option1Value      = "White",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // --- Product 9: Side Pod Set (2 variants: Color Black/White) ---

                var sidePodSet = new Product
                {
                    Title             = "Side Pod Set",
                    Handle            = "side-pod-set",
                    VendorID          = vendorOtk.VendorID,
                    ProductCategoryID = catBodywork.CategoryID,
                    ProductTypeID     = typeBodywork.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(sidePodSet);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = sidePodSet.ProductID,
                        SKU               = "SEED-SP-BLK",
                        Price             = 120.00m,
                        InventoryQuantity = 12,
                        IsDefault         = true,
                        Option1Name       = "Color",
                        Option1Value      = "Black",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = sidePodSet.ProductID,
                        SKU               = "SEED-SP-WHT",
                        Price             = 120.00m,
                        InventoryQuantity = 12,
                        IsDefault         = false,
                        Option1Name       = "Color",
                        Option1Value      = "White",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // --- Product 10: Mychron 5 Lap Timer (standalone) ---

                var mychron5 = new Product
                {
                    Title             = "Mychron 5 Lap Timer",
                    Handle            = "mychron-5-lap-timer",
                    VendorID          = vendorMychron.VendorID,
                    ProductCategoryID = catElectronics.CategoryID,
                    ProductTypeID     = typeElectronics.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(mychron5);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = mychron5.ProductID,
                    SKU               = "SEED-MC5-001",
                    Price             = 449.99m,
                    InventoryQuantity = 8,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 11: Chain Lube 500ml (standalone) ---

                var chainLube = new Product
                {
                    Title             = "Chain Lube 500ml",
                    Handle            = "chain-lube-500ml",
                    VendorID          = vendorCrg.VendorID,
                    ProductCategoryID = catLubricants.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(chainLube);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = chainLube.ProductID,
                    SKU               = "SEED-CL-001",
                    Price             = 12.99m,
                    InventoryQuantity = 100,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 12: Kart Stand (standalone) ---

                var kartStand = new Product
                {
                    Title             = "Kart Stand",
                    Handle            = "kart-stand",
                    VendorID          = vendorTonyKart.VendorID,
                    ProductCategoryID = catAccessories.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(kartStand);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = kartStand.ProductID,
                    SKU               = "SEED-KS-001",
                    Price             = 79.99m,
                    InventoryQuantity = 15,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 13: Steering Wheel 300mm (standalone) ---

                var steeringWheel = new Product
                {
                    Title             = "Steering Wheel 300mm",
                    Handle            = "steering-wheel-300mm",
                    VendorID          = vendorCrg.VendorID,
                    ProductCategoryID = catAccessories.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(steeringWheel);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = steeringWheel.ProductID,
                    SKU               = "SEED-SW-001",
                    Price             = 65.00m,
                    InventoryQuantity = 20,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 14: Racing Suit (3 variants: Size S/M/L) ---

                var racingSuit = new Product
                {
                    Title             = "Racing Suit",
                    Handle            = "racing-suit",
                    VendorID          = vendorSparco.VendorID,
                    ProductCategoryID = catSafety.CategoryID,
                    ProductTypeID     = typePart.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(racingSuit);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = racingSuit.ProductID,
                        SKU               = "SEED-RS-S",
                        Price             = 299.99m,
                        InventoryQuantity = 10,
                        IsDefault         = true,
                        Option1Name       = "Size",
                        Option1Value      = "S",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = racingSuit.ProductID,
                        SKU               = "SEED-RS-M",
                        Price             = 299.99m,
                        InventoryQuantity = 10,
                        IsDefault         = false,
                        Option1Name       = "Size",
                        Option1Value      = "M",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = racingSuit.ProductID,
                        SKU               = "SEED-RS-L",
                        Price             = 299.99m,
                        InventoryQuantity = 10,
                        IsDefault         = false,
                        Option1Name       = "Size",
                        Option1Value      = "L",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // --- Product 15: Rotax Max Engine (standalone, draft) ---

                var rotaxMax = new Product
                {
                    Title             = "Rotax Max Engine",
                    Handle            = "rotax-max-engine",
                    VendorID          = vendorRotax.VendorID,
                    ProductCategoryID = catEngines.CategoryID,
                    ProductTypeID     = typeEngine.ProductTypeID,
                    Status            = ProductStatus.Draft
                };

                context.Products.Add(rotaxMax);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = rotaxMax.ProductID,
                    SKU               = "SEED-RMX-001",
                    Price             = 2499.99m,
                    InventoryQuantity = 3,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Draft
                });

                context.SaveChanges();

                // --- Product 16: Birel ART Chassis (standalone) ---

                var birelChassis = new Product
                {
                    Title             = "Birel ART Chassis",
                    Handle            = "birel-art-chassis",
                    VendorID          = vendorBirelArt.VendorID,
                    ProductCategoryID = catChassis.CategoryID,
                    ProductTypeID     = typeFrame.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(birelChassis);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = birelChassis.ProductID,
                    SKU               = "SEED-BAC-001",
                    Price             = 3200.00m,
                    InventoryQuantity = 2,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 17: Tony Kart Racer 401R (standalone) ---

                var tonyRacer = new Product
                {
                    Title             = "Tony Kart Racer 401R",
                    Handle            = "tony-kart-racer-401r",
                    VendorID          = vendorTonyKart.VendorID,
                    ProductCategoryID = catChassis.CategoryID,
                    ProductTypeID     = typeFrame.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(tonyRacer);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = tonyRacer.ProductID,
                    SKU               = "SEED-TKR-001",
                    Price             = 4100.00m,
                    InventoryQuantity = 2,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 18: Bridgestone YDP Tire (2 variants: Position Front/Rear) ---

                var bridgestoneYdp = new Product
                {
                    Title             = "Bridgestone YDP Tire",
                    Handle            = "bridgestone-ydp-tire",
                    VendorID          = vendorBridgestone.VendorID,
                    ProductCategoryID = catTires.CategoryID,
                    ProductTypeID     = typeTire.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(bridgestoneYdp);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = bridgestoneYdp.ProductID,
                        SKU               = "SEED-BYF-001",
                        Price             = 54.99m,
                        InventoryQuantity = 30,
                        IsDefault         = true,
                        Option1Name       = "Position",
                        Option1Value      = "Front",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = bridgestoneYdp.ProductID,
                        SKU               = "SEED-BYR-001",
                        Price             = 54.99m,
                        InventoryQuantity = 30,
                        IsDefault         = false,
                        Option1Name       = "Position",
                        Option1Value      = "Rear",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // --- Product 19: Alfano Pro Temp Sensor (standalone) ---

                var alfanoSensor = new Product
                {
                    Title             = "Alfano Pro Temp Sensor",
                    Handle            = "alfano-pro-temp-sensor",
                    VendorID          = vendorAlfano.VendorID,
                    ProductCategoryID = catElectronics.CategoryID,
                    ProductTypeID     = typeElectronics.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(alfanoSensor);
                context.SaveChanges();

                context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = alfanoSensor.ProductID,
                    SKU               = "SEED-ATS-001",
                    Price             = 89.99m,
                    InventoryQuantity = 12,
                    IsDefault         = true,
                    Option1Name       = "Title",
                    Option1Value      = "Default Title",
                    Status            = ProductStatus.Published
                });

                context.SaveChanges();

                // --- Product 20: Tillett T11 Seat (3 variants: Size XS/S/M) ---

                var tillettSeat = new Product
                {
                    Title             = "Tillett T11 Seat",
                    Handle            = "tillett-t11-seat",
                    VendorID          = vendorTillett.VendorID,
                    ProductCategoryID = catAccessories.CategoryID,
                    ProductTypeID     = typeSeat.ProductTypeID,
                    Status            = ProductStatus.Published
                };

                context.Products.Add(tillettSeat);
                context.SaveChanges();

                context.ProductVariants.AddRange(
                    new ProductVariant
                    {
                        ProductID         = tillettSeat.ProductID,
                        SKU               = "SEED-T11-XS",
                        Price             = 185.00m,
                        InventoryQuantity = 8,
                        IsDefault         = true,
                        Option1Name       = "Size",
                        Option1Value      = "XS",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = tillettSeat.ProductID,
                        SKU               = "SEED-T11-S",
                        Price             = 185.00m,
                        InventoryQuantity = 8,
                        IsDefault         = false,
                        Option1Name       = "Size",
                        Option1Value      = "S",
                        Status            = ProductStatus.Published
                    },
                    new ProductVariant
                    {
                        ProductID         = tillettSeat.ProductID,
                        SKU               = "SEED-T11-M",
                        Price             = 185.00m,
                        InventoryQuantity = 8,
                        IsDefault         = false,
                        Option1Name       = "Size",
                        Option1Value      = "M",
                        Status            = ProductStatus.Published
                    }
                );

                context.SaveChanges();

                // ===== SEED 6 — ADDRESSES (15 total) =====

                var addresses = new List<Address>
                {
                    // Canadian — Ontario
                    new Address { Street = "100 King St W",           City = "Toronto",       Province = "Ontario",          PostalCode = "M5V 1A1", Country = "Canada"        }, // [0]
                    new Address { Street = "250 City Centre Dr",      City = "Mississauga",   Province = "Ontario",          PostalCode = "L5A 1X3", Country = "Canada"        }, // [1]
                    new Address { Street = "45 Sparks St",            City = "Ottawa",        Province = "Ontario",          PostalCode = "K1A 0A9", Country = "Canada"        }, // [2]
                    new Address { Street = "78 Main St E",            City = "Hamilton",      Province = "Ontario",          PostalCode = "L8H 2L4", Country = "Canada"        }, // [3]
                    // Canadian — Quebec
                    new Address { Street = "1 Place Ville Marie",     City = "Montreal",      Province = "Quebec",           PostalCode = "H2X 1V5", Country = "Canada"        }, // [4]
                    new Address { Street = "320 Grande Allée E",      City = "Quebec City",   Province = "Quebec",           PostalCode = "G1R 4A5", Country = "Canada"        }, // [5]
                    // Canadian — British Columbia
                    new Address { Street = "555 Burrard St",          City = "Vancouver",     Province = "British Columbia", PostalCode = "V6B 1A1", Country = "Canada"        }, // [6]
                    new Address { Street = "9855 King George Blvd",   City = "Surrey",        Province = "British Columbia", PostalCode = "V3R 2N4", Country = "Canada"        }, // [7]
                    // Canadian — Alberta
                    new Address { Street = "350 7th Ave SW",          City = "Calgary",       Province = "Alberta",          PostalCode = "T2P 1J9", Country = "Canada"        }, // [8]
                    new Address { Street = "10055 106 St NW",         City = "Edmonton",      Province = "Alberta",          PostalCode = "T5J 1N4", Country = "Canada"        }, // [9]
                    // Canadian — Ontario (extras)
                    new Address { Street = "7895 Torbram Rd",         City = "Brampton",      Province = "Ontario",          PostalCode = "L6T 3Y3", Country = "Canada"        }, // [10]
                    new Address { Street = "3000 Le Corbusier Blvd",  City = "Laval",         Province = "Quebec",           PostalCode = "H7G 1A8", Country = "Canada"        }, // [11]
                    // US
                    new Address { Street = "1200 E Jefferson Ave",    City = "Detroit",       Province = "Michigan",         PostalCode = "48207",   Country = "United States" }, // [12]
                    new Address { Street = "440 N Michigan Ave",      City = "Chicago",       Province = "Illinois",         PostalCode = "60611",   Country = "United States" }, // [13]
                    new Address { Street = "600 Pine St",             City = "Seattle",       Province = "Washington",       PostalCode = "98101",   Country = "United States" }  // [14]
                };

                context.Addresses.AddRange(addresses);
                context.SaveChanges();

                // ===== SEED 7 — DEALERS (8 total) =====

                var ptCod   = paymentTerms.First(p => p.TermCode == "COD");
                var ptNet30 = paymentTerms.First(p => p.TermCode == "NET30");
                var ptNet60 = paymentTerms.First(p => p.TermCode == "NET60");

                var dealers = new List<Dealer>
                {
                    new Dealer // [0] Speed King Motorsports — Toronto, Net30
                    {
                        CompanyName      = "Speed King Motorsports",
                        ContactPhone     = "416-555-0101",
                        BillingAddressID = addresses[0].AddressID,
                        PaymentTermsID   = ptNet30.PaymentTermsID,
                        IsTaxExempt      = false
                    },
                    new Dealer // [1] Track Day Canada — Montreal, Net60
                    {
                        CompanyName      = "Track Day Canada",
                        ContactPhone     = "514-555-0202",
                        BillingAddressID = addresses[4].AddressID,
                        PaymentTermsID   = ptNet60.PaymentTermsID,
                        IsTaxExempt      = false
                    },
                    new Dealer // [2] Fast Lane Racing USA — Detroit, COD, TaxExempt
                    {
                        CompanyName      = "Fast Lane Racing USA",
                        ContactPhone     = "313-555-0303",
                        BillingAddressID = addresses[12].AddressID,
                        PaymentTermsID   = ptCod.PaymentTermsID,
                        IsTaxExempt      = true
                    },
                    new Dealer // [3] Apex Kart Supply — Vancouver, Net30
                    {
                        CompanyName      = "Apex Kart Supply",
                        ContactPhone     = "604-555-0404",
                        BillingAddressID = addresses[6].AddressID,
                        PaymentTermsID   = ptNet30.PaymentTermsID,
                        IsTaxExempt      = false
                    },
                    new Dealer // [4] Podium Performance — Calgary, Net60
                    {
                        CompanyName      = "Podium Performance",
                        ContactPhone     = "403-555-0505",
                        BillingAddressID = addresses[8].AddressID,
                        PaymentTermsID   = ptNet60.PaymentTermsID,
                        IsTaxExempt      = false
                    },
                    new Dealer // [5] Grid Position Racing — Chicago, Net30, TaxExempt
                    {
                        CompanyName      = "Grid Position Racing",
                        ContactPhone     = "312-555-0606",
                        BillingAddressID = addresses[13].AddressID,
                        PaymentTermsID   = ptNet30.PaymentTermsID,
                        IsTaxExempt      = true
                    },
                    new Dealer // [6] Checkered Flag Parts — Ottawa, COD
                    {
                        CompanyName      = "Checkered Flag Parts",
                        ContactPhone     = "613-555-0707",
                        BillingAddressID = addresses[2].AddressID,
                        PaymentTermsID   = ptCod.PaymentTermsID,
                        IsTaxExempt      = false
                    },
                    new Dealer // [7] Turn One Motorsports — Seattle, Net60, TaxExempt
                    {
                        CompanyName      = "Turn One Motorsports",
                        ContactPhone     = "206-555-0808",
                        BillingAddressID = addresses[14].AddressID,
                        PaymentTermsID   = ptNet60.PaymentTermsID,
                        IsTaxExempt      = true
                    }
                };

                context.Dealers.AddRange(dealers);
                context.SaveChanges();

                // ===== SEED 8 — ORDERS + ORDER LINES + ORDER HISTORY =====

                // Variant lookup by SKU (includes Product navigation)
                var vm = context.ProductVariants
                    .Include(v => v.Product)
                    .ToDictionary(v => v.SKU);

                // Helper: build an OrderLine from a SKU, quantity, overriding VariantTitle for multi-variant
                OrderLine Line(int orderId, string sku, int qty)
                {
                    var v = vm[sku];
                    var variantTitle = v.Option1Value == "Default Title" ? null : v.Option1Value;
                    return new OrderLine
                    {
                        OrderID          = orderId,
                        ProductVariantID = v.VariantID,
                        SKU              = v.SKU,
                        ProductTitle     = v.Product!.Title,
                        VariantTitle     = variantTitle,
                        Quantity         = qty,
                        UnitPrice        = v.Price,
                        LineTotal        = qty * v.Price
                    };
                }

                // Helper: build an OrderHistory entry
                OrderHistory Hist(int orderId, int versionNum, string changeType, string desc, DateTime changedDate)
                {
                    return new OrderHistory
                    {
                        OrderID           = orderId,
                        VersionNumber     = versionNum,
                        ChangeType        = changeType,
                        ChangeDescription = desc,
                        ChangedBy         = "system.seed",
                        ChangedDate       = changedDate
                    };
                }

                // ---- Orders 0001–0003: Submitted ----

                var order0001 = new Order
                {
                    OrderNumber        = "0001",
                    DealerID           = dealers[0].DealerID,
                    DealerCompanyName  = dealers[0].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 2, 11, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 299.97m,
                    TotalAmount        = 299.97m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 4, 11, 0, 0, DateTimeKind.Utc),
                    Status             = "Submitted",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0002 = new Order
                {
                    OrderNumber        = "0002",
                    DealerID           = dealers[1].DealerID,
                    DealerCompanyName  = dealers[1].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 7, 14, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 719.92m,
                    TotalAmount        = 719.92m,
                    PaymentTermsID     = ptNet60.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 4, 8, 14, 0, 0, DateTimeKind.Utc),
                    Status             = "Submitted",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0003 = new Order
                {
                    OrderNumber        = "0003",
                    DealerID           = dealers[2].DealerID,
                    DealerCompanyName  = dealers[2].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 12, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 2569.97m,
                    TotalAmount        = 2569.97m,
                    PaymentTermsID     = ptCod.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 2, 12, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Submitted",
                    IsTaxExempt        = true,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                // ---- Orders 0004–0006: Confirmed ----

                var order0004 = new Order
                {
                    OrderNumber        = "0004",
                    DealerID           = dealers[3].DealerID,
                    DealerCompanyName  = dealers[3].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 13, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 668.94m,
                    TotalAmount        = 668.94m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Confirmed",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0005 = new Order
                {
                    OrderNumber        = "0005",
                    DealerID           = dealers[4].DealerID,
                    DealerCompanyName  = dealers[4].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 16, 9, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 391.98m,
                    TotalAmount        = 391.98m,
                    PaymentTermsID     = ptNet60.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 4, 17, 9, 0, 0, DateTimeKind.Utc),
                    Status             = "Confirmed",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0006 = new Order
                {
                    OrderNumber        = "0006",
                    DealerID           = dealers[0].DealerID,
                    DealerCompanyName  = dealers[0].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 19, 14, 30, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 4100.00m,
                    TotalAmount        = 4100.00m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 21, 14, 30, 0, DateTimeKind.Utc),
                    Status             = "Confirmed",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                // ---- Orders 0007–0009: Shipped ----

                var order0007 = new Order
                {
                    OrderNumber        = "0007",
                    DealerID           = dealers[1].DealerID,
                    DealerCompanyName  = dealers[1].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 784.98m,
                    TotalAmount        = 784.98m,
                    PaymentTermsID     = ptNet60.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 4, 18, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Shipped",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0,
                    TrackingNumber     = "1Z999AA10123456784"
                };

                var order0008 = new Order
                {
                    OrderNumber        = "0008",
                    DealerID           = dealers[5].DealerID,
                    DealerCompanyName  = dealers[5].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 19, 9, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 599.90m,
                    TotalAmount        = 599.90m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 21, 9, 0, 0, DateTimeKind.Utc),
                    Status             = "Shipped",
                    IsTaxExempt        = true,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0,
                    TrackingNumber     = "1Z999AA10123456785"
                };

                var order0009 = new Order
                {
                    OrderNumber        = "0009",
                    DealerID           = dealers[2].DealerID,
                    DealerCompanyName  = dealers[2].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 3379.98m,
                    TotalAmount        = 3379.98m,
                    PaymentTermsID     = ptCod.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Shipped",
                    IsTaxExempt        = true,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0,
                    TrackingNumber     = "1Z999AA10123456786"
                };

                // ---- Orders 0010–0011: Invoiced ----

                var order0010 = new Order
                {
                    OrderNumber        = "0010",
                    DealerID           = dealers[6].DealerID,
                    DealerCompanyName  = dealers[6].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 16, 11, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 764.96m,
                    TotalAmount        = 764.96m,
                    PaymentTermsID     = ptCod.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 2, 16, 11, 0, 0, DateTimeKind.Utc),
                    Status             = "Invoiced",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0011 = new Order
                {
                    OrderNumber        = "0011",
                    DealerID           = dealers[3].DealerID,
                    DealerCompanyName  = dealers[3].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 1079.88m,
                    TotalAmount        = 1079.88m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 19, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Invoiced",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                // ---- Orders 0012–0013: ActionRequired ----

                var order0012 = new Order
                {
                    OrderNumber        = "0012",
                    DealerID           = dealers[4].DealerID,
                    DealerCompanyName  = dealers[4].CompanyName,
                    OrderDate          = new DateTime(2026, 2, 26, 11, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 1325.97m,
                    TotalAmount        = 1325.97m,
                    PaymentTermsID     = ptNet60.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 4, 27, 11, 0, 0, DateTimeKind.Utc),
                    Status             = "ActionRequired",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0013 = new Order
                {
                    OrderNumber        = "0013",
                    DealerID           = dealers[7].DealerID,
                    DealerCompanyName  = dealers[7].CompanyName,
                    OrderDate          = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 4285.00m,
                    TotalAmount        = 4285.00m,
                    PaymentTermsID     = ptNet60.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 4, 30, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "ActionRequired",
                    IsTaxExempt        = true,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                // ---- Orders 0014–0015: Cancelled ----

                var order0014 = new Order
                {
                    OrderNumber        = "0014",
                    DealerID           = dealers[5].DealerID,
                    DealerCompanyName  = dealers[5].CompanyName,
                    OrderDate          = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 391.98m,
                    TotalAmount        = 391.98m,
                    PaymentTermsID     = ptNet30.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 31, 9, 0, 0, DateTimeKind.Utc),
                    Status             = "Cancelled",
                    IsTaxExempt        = true,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                var order0015 = new Order
                {
                    OrderNumber        = "0015",
                    DealerID           = dealers[6].DealerID,
                    DealerCompanyName  = dealers[6].CompanyName,
                    OrderDate          = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc),
                    SubtotalAmount     = 537.95m,
                    TotalAmount        = 537.95m,
                    PaymentTermsID     = ptCod.PaymentTermsID,
                    PaymentDueDate     = new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc),
                    Status             = "Cancelled",
                    IsTaxExempt        = false,
                    IsCurrentVersion   = true,
                    VersionNumber      = 0
                };

                context.Orders.AddRange(
                    order0001, order0002, order0003,
                    order0004, order0005, order0006,
                    order0007, order0008, order0009,
                    order0010, order0011,
                    order0012, order0013,
                    order0014, order0015
                );
                context.SaveChanges();

                // ---- Order Lines ----

                var lines = new List<OrderLine>
                {
                    // 0001 — Submitted (Speed King)
                    Line(order0001.OrderID, "SEED-RG-001",   2),
                    Line(order0001.OrderID, "SEED-KH-001",   1),

                    // 0002 — Submitted (Track Day)
                    Line(order0002.OrderID, "MG-TY-S",       4),
                    Line(order0002.OrderID, "MG-TY-M",       4),

                    // 0003 — Submitted (Fast Lane USA)
                    Line(order0003.OrderID, "SEED-RMX-001",  1),
                    Line(order0003.OrderID, "SEED-BD-001",   2),

                    // 0004 — Confirmed (Apex Kart)
                    Line(order0004.OrderID, "SEED-MC5-001",  1),
                    Line(order0004.OrderID, "SEED-ATS-001",  2),
                    Line(order0004.OrderID, "SEED-CL-001",   3),

                    // 0005 — Confirmed (Podium Performance)
                    Line(order0005.OrderID, "SEED-FF-BLK",   2),
                    Line(order0005.OrderID, "SEED-SP-BLK",   2),

                    // 0006 — Confirmed (Speed King)
                    Line(order0006.OrderID, "SEED-TKR-001",  1),

                    // 0007 — Shipped (Track Day)
                    Line(order0007.OrderID, "SEED-RS-M",     2),
                    Line(order0007.OrderID, "SEED-T11-S",    1),

                    // 0008 — Shipped (Grid Position)
                    Line(order0008.OrderID, "SEED-BYF-001",  4),
                    Line(order0008.OrderID, "SEED-BYR-001",  4),
                    Line(order0008.OrderID, "SEED-KS-001",   2),

                    // 0009 — Shipped (Fast Lane USA)
                    Line(order0009.OrderID, "SEED-BAC-001",  1),
                    Line(order0009.OrderID, "SEED-BCK-001",  2),

                    // 0010 — Invoiced (Checkered Flag)
                    Line(order0010.OrderID, "SEED-RG-001",   2),
                    Line(order0010.OrderID, "SEED-RS-S",     2),
                    Line(order0010.OrderID, "SEED-SW-001",   1),

                    // 0011 — Invoiced (Apex Kart)
                    Line(order0011.OrderID, "MG-TY-S",       6),
                    Line(order0011.OrderID, "MG-TY-M",       6),

                    // 0012 — ActionRequired (Podium Performance)
                    Line(order0012.OrderID, "VTX-ROK-GP-001", 1),
                    Line(order0012.OrderID, "SEED-CL-001",    2),

                    // 0013 — ActionRequired (Turn One)
                    Line(order0013.OrderID, "SEED-TKR-001",  1),
                    Line(order0013.OrderID, "SEED-T11-M",    1),

                    // 0014 — Cancelled (Grid Position)
                    Line(order0014.OrderID, "SEED-FF-WHT",   2),
                    Line(order0014.OrderID, "SEED-SP-WHT",   2),

                    // 0015 — Cancelled (Checkered Flag)
                    Line(order0015.OrderID, "CRG-NB-001",    3),
                    Line(order0015.OrderID, "SEED-KH-001",   2)
                };

                context.OrderLines.AddRange(lines);
                context.SaveChanges();

                // ---- Order History ----

                var history = new List<OrderHistory>
                {
                    // 0001 — Submitted
                    Hist(order0001.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 1,  10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0001.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 2,  11, 0, 0, DateTimeKind.Utc)),

                    // 0002 — Submitted
                    Hist(order0002.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 6,  9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0002.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 7,  14, 0, 0, DateTimeKind.Utc)),

                    // 0003 — Submitted
                    Hist(order0003.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 11, 11, 30, 0, DateTimeKind.Utc)),
                    Hist(order0003.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 12, 10, 0, 0, DateTimeKind.Utc)),

                    // 0004 — Confirmed
                    Hist(order0004.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 12, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0004.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 13, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0004.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 14, 11, 0, 0, DateTimeKind.Utc)),

                    // 0005 — Confirmed
                    Hist(order0005.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 15, 8,  30, 0, DateTimeKind.Utc)),
                    Hist(order0005.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 16, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0005.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 17, 10, 30, 0, DateTimeKind.Utc)),

                    // 0006 — Confirmed
                    Hist(order0006.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 19, 14, 0, 0, DateTimeKind.Utc)),
                    Hist(order0006.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 19, 14, 30, 0, DateTimeKind.Utc)),
                    Hist(order0006.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 20, 9,  0, 0, DateTimeKind.Utc)),

                    // 0007 — Shipped
                    Hist(order0007.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 16, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0007.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0007.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 19, 11, 0, 0, DateTimeKind.Utc)),
                    Hist(order0007.OrderID, 0, "Shipped",   "Order shipped — tracking 1Z999AA10123456784", new DateTime(2026, 2, 22, 14, 0, 0, DateTimeKind.Utc)),

                    // 0008 — Shipped
                    Hist(order0008.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 18, 8,  0, 0, DateTimeKind.Utc)),
                    Hist(order0008.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 19, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0008.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 21, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0008.OrderID, 0, "Shipped",   "Order shipped — tracking 1Z999AA10123456785", new DateTime(2026, 2, 24, 13, 0, 0, DateTimeKind.Utc)),

                    // 0009 — Shipped
                    Hist(order0009.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 19, 11, 0, 0, DateTimeKind.Utc)),
                    Hist(order0009.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0009.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 22, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0009.OrderID, 0, "Shipped",   "Order shipped — tracking 1Z999AA10123456786", new DateTime(2026, 2, 25, 15, 0, 0, DateTimeKind.Utc)),

                    // 0010 — Invoiced
                    Hist(order0010.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 15, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0010.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 16, 11, 0, 0, DateTimeKind.Utc)),
                    Hist(order0010.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 18, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0010.OrderID, 0, "Shipped",   "Order shipped",                              new DateTime(2026, 2, 22, 14, 0, 0, DateTimeKind.Utc)),
                    Hist(order0010.OrderID, 0, "Invoiced",  "Invoice issued to dealer",                   new DateTime(2026, 2, 26, 10, 0, 0, DateTimeKind.Utc)),

                    // 0011 — Invoiced
                    Hist(order0011.OrderID, 0, "Created",   "Order created by dealer",                    new DateTime(2026, 2, 16, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0011.OrderID, 0, "Submitted", "Order submitted for processing",              new DateTime(2026, 2, 17, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0011.OrderID, 0, "Confirmed", "Order confirmed by Pontello admin",           new DateTime(2026, 2, 19, 11, 0, 0, DateTimeKind.Utc)),
                    Hist(order0011.OrderID, 0, "Shipped",   "Order shipped",                              new DateTime(2026, 2, 23, 14, 0, 0, DateTimeKind.Utc)),
                    Hist(order0011.OrderID, 0, "Invoiced",  "Invoice issued to dealer",                   new DateTime(2026, 2, 27, 10, 0, 0, DateTimeKind.Utc)),

                    // 0012 — ActionRequired
                    Hist(order0012.OrderID, 0, "Created",        "Order created by dealer",               new DateTime(2026, 2, 25, 10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0012.OrderID, 0, "Submitted",      "Order submitted for processing",        new DateTime(2026, 2, 26, 11, 0, 0, DateTimeKind.Utc)),
                    Hist(order0012.OrderID, 0, "ActionRequired", "Action required: engine backorder",     new DateTime(2026, 3, 1,  9,  0, 0, DateTimeKind.Utc)),

                    // 0013 — ActionRequired
                    Hist(order0013.OrderID, 0, "Created",        "Order created by dealer",               new DateTime(2026, 2, 28, 9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0013.OrderID, 0, "Submitted",      "Order submitted for processing",        new DateTime(2026, 3, 1,  10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0013.OrderID, 0, "ActionRequired", "Action required: chassis lead time",    new DateTime(2026, 3, 2,  11, 0, 0, DateTimeKind.Utc)),

                    // 0014 — Cancelled
                    Hist(order0014.OrderID, 0, "Created",    "Order created by dealer",                   new DateTime(2026, 2, 28, 14, 0, 0, DateTimeKind.Utc)),
                    Hist(order0014.OrderID, 0, "Submitted",  "Order submitted for processing",            new DateTime(2026, 3, 1,  9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0014.OrderID, 0, "Cancelled",  "Order cancelled by dealer",                 new DateTime(2026, 3, 3,  10, 0, 0, DateTimeKind.Utc)),

                    // 0015 — Cancelled
                    Hist(order0015.OrderID, 0, "Created",    "Order created by dealer",                   new DateTime(2026, 3, 3,  9,  0, 0, DateTimeKind.Utc)),
                    Hist(order0015.OrderID, 0, "Submitted",  "Order submitted for processing",            new DateTime(2026, 3, 3,  10, 0, 0, DateTimeKind.Utc)),
                    Hist(order0015.OrderID, 0, "Cancelled",  "Order cancelled — product discontinued",   new DateTime(2026, 3, 4,  9,  0, 0, DateTimeKind.Utc))
                };

                context.OrderHistories.AddRange(history);
                context.SaveChanges();
            }
        }
    }
}
