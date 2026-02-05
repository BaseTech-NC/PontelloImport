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

				// Ensure database is created
				context.Database.EnsureCreated();

				// ===== SEED VENDORS =====
				if (!context.Vendors.Any())
					{
					var vendors = new List<Vendor>
					{
						new Vendor
						{
							VendorName = "Birel ART",
							VendorSlug = "birel-art",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "Vortex Engines",
							VendorSlug = "vortex-engines",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "MG Tires",
							VendorSlug = "mg-tires",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "CRG",
							VendorSlug = "crg",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "Tony Kart",
							VendorSlug = "tony-kart",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "OTK Kart Group",
							VendorSlug = "otk-kart-group",
							Country = "Italy",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "Pontello Motorsports",
							VendorSlug = "pontello-motorsports",
							Country = "Canada",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "Phantom Racing Chassis",
							VendorSlug = "phantom-racing-chassis",
							Country = "USA",
							IsActive = true
						},
						new Vendor
						{
							VendorName = "Authentic Phantom Component",
							VendorSlug = "authentic-phantom-component",
							Country = "USA",
							IsActive = true
						}
					};

					context.Vendors.AddRange(vendors);
					context.SaveChanges();
					}

				// ===== SEED PRODUCT CATEGORIES =====
				if (!context.ProductCategories.Any())
					{
					var categories = new List<ProductCategory>
					{
						new ProductCategory
						{
							CategoryName = "Chassis",
							CategorySlug = "chassis",
							CategoryDescription = "Complete kart frames and chassis components",
							DisplayOrder = 1,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Tires",
							CategorySlug = "tires",
							CategoryDescription = "Racing tires for all conditions",
							DisplayOrder = 2,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Engines",
							CategorySlug = "engines",
							CategoryDescription = "Kart racing engines and motor components",
							DisplayOrder = 3,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Accessories",
							CategorySlug = "accessories",
							CategoryDescription = "Kart accessories, parts, and components",
							DisplayOrder = 4,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Safety Equipment",
							CategorySlug = "safety-equipment",
							CategoryDescription = "Helmets, suits, gloves, and safety gear",
							DisplayOrder = 5,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Tools",
							CategorySlug = "tools",
							CategoryDescription = "Maintenance and setup tools",
							DisplayOrder = 6,
							IsActive = true
						}
					};

					context.ProductCategories.AddRange(categories);
					context.SaveChanges();
					}

				// ===== SEED PRODUCTS (Parent containers) =====
				if (!context.Products.Any())
					{
					var products = new List<Product>
					{
						new Product
						{
							Title = "Nerf Bar",
							Handle = "nerf-bar",
							VendorID = 1, // Birel ART
                            ProductCategoryID = 1, // Chassis
                            Description = "Heavy-duty nerf bar for chassis protection. Available in powder coated or stainless steel finish.",
							Type = "Chassis Component",
							IsActive = true
						},
						new Product
						{
							Title = "Racing Steering Wheel",
							Handle = "racing-steering-wheel",
							VendorID = 4, // CRG
                            ProductCategoryID = 4, // Accessories
                            Description = "Ergonomic racing steering wheel with rubber grip. Multiple diameter options available.",
							Type = "Steering Component",
							IsActive = true
						},
						new Product
						{
							Title = "Tie Rod Assembly",
							Handle = "tie-rod-assembly",
							VendorID = 5, // Tony Kart
                            ProductCategoryID = 1, // Chassis
                            Description = "Adjustable tie rod assembly for precise steering alignment.",
							Type = "Steering Component",
							IsActive = true
						}
					};

					context.Products.AddRange(products);
					context.SaveChanges();
					}

				// ===== SEED PRODUCT VARIANTS =====
				if (!context.ProductVariants.Any())
					{
					var variants = new List<ProductVariant>();

					// Get product IDs (they were just created above)
					var nerfBarProduct = context.Products.First(p => p.Handle == "nerf-bar");
					var steeringWheelProduct = context.Products.First(p => p.Handle == "racing-steering-wheel");
					var tieRodProduct = context.Products.First(p => p.Handle == "tie-rod-assembly");

					// Nerf Bar Variants (4 variants: Material × Side)
					variants.Add(new ProductVariant
						{
						ProductID = nerfBarProduct.ProductID,
						Title = "Nerf Bar - Powder Coated Left",
						Handle = "nerf-bar-powder-coated-left",
						SKU = "PROD-1001",
						Price = 72.32M,
						InventoryQuantity = 15,
						Weight = 800,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = nerfBarProduct.ProductID,
						Title = "Nerf Bar - Powder Coated Right",
						Handle = "nerf-bar-powder-coated-right",
						SKU = "PROD-1002",
						Price = 72.32M,
						InventoryQuantity = 12,
						Weight = 800,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = nerfBarProduct.ProductID,
						Title = "Nerf Bar - Stainless Steel Left",
						Handle = "nerf-bar-stainless-steel-left",
						SKU = "PROD-1003",
						Price = 97.18M,
						InventoryQuantity = 8,
						Weight = 900,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = nerfBarProduct.ProductID,
						Title = "Nerf Bar - Stainless Steel Right",
						Handle = "nerf-bar-stainless-steel-right",
						SKU = "PROD-1004",
						Price = 97.18M,
						InventoryQuantity = 10,
						Weight = 900,
						IsActive = true
						});

					// Steering Wheel Variants (4 variants: Color × Diameter)
					variants.Add(new ProductVariant
						{
						ProductID = steeringWheelProduct.ProductID,
						Title = "Racing Steering Wheel - Black 10 inch",
						Handle = "racing-steering-wheel-black-10in",
						SKU = "PROD-2001",
						Price = 129.99M,
						InventoryQuantity = 20,
						Weight = 500,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = steeringWheelProduct.ProductID,
						Title = "Racing Steering Wheel - Black 12 inch",
						Handle = "racing-steering-wheel-black-12in",
						SKU = "PROD-2002",
						Price = 149.99M,
						InventoryQuantity = 15,
						Weight = 650,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = steeringWheelProduct.ProductID,
						Title = "Racing Steering Wheel - Red 10 inch",
						Handle = "racing-steering-wheel-red-10in",
						SKU = "PROD-2003",
						Price = 139.99M,
						InventoryQuantity = 18,
						Weight = 500,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = steeringWheelProduct.ProductID,
						Title = "Racing Steering Wheel - Red 12 inch",
						Handle = "racing-steering-wheel-red-12in",
						SKU = "PROD-2004",
						Price = 159.99M,
						InventoryQuantity = 12,
						Weight = 650,
						IsActive = true
						});

					// Tie Rod Variants (2 variants: Length)
					variants.Add(new ProductVariant
						{
						ProductID = tieRodProduct.ProductID,
						Title = "Tie Rod Assembly - 6 inch",
						Handle = "tie-rod-assembly-6in",
						SKU = "PROD-3001",
						Price = 45.50M,
						InventoryQuantity = 25,
						Weight = 300,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = tieRodProduct.ProductID,
						Title = "Tie Rod Assembly - 6.5 inch",
						Handle = "tie-rod-assembly-6-5in",
						SKU = "PROD-3002",
						Price = 47.50M,
						InventoryQuantity = 22,
						Weight = 320,
						IsActive = true
						});

					// STANDALONE PRODUCTS (ProductID = null)
					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Floor Pan Bolt Kit - Phantom",
						Handle = "floor-pan-bolt-kit-phantom",
						SKU = "PRC-1130280",
						Price = 10.20M,
						InventoryQuantity = 50,
						Weight = 150,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Chassis Cleaning Spray",
						Handle = "chassis-cleaning-spray",
						SKU = "PROD-5001",
						Price = 18.99M,
						InventoryQuantity = 40,
						Weight = 500,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Racing Gloves - Medium",
						Handle = "racing-gloves-medium",
						SKU = "PROD-6001",
						Price = 65.00M,
						InventoryQuantity = 30,
						Weight = 200,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Racing Gloves - Large",
						Handle = "racing-gloves-large",
						SKU = "PROD-6002",
						Price = 65.00M,
						InventoryQuantity = 35,
						Weight = 210,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Torque Wrench - 40Nm",
						Handle = "torque-wrench-40nm",
						SKU = "PROD-7001",
						Price = 89.99M,
						InventoryQuantity = 12,
						Weight = 800,
						IsActive = true
						});

					variants.Add(new ProductVariant
						{
						ProductID = null, // Standalone
						Title = "Tire Pressure Gauge Digital",
						Handle = "tire-pressure-gauge-digital",
						SKU = "PROD-7002",
						Price = 45.50M,
						InventoryQuantity = 18,
						Weight = 150,
						IsActive = true
						});

					context.ProductVariants.AddRange(variants);
					context.SaveChanges();
					}

				// ===== SEED PRODUCT ATTRIBUTES =====
				if (!context.ProductAttributes.Any())
					{
					var attributes = new List<ProductAttribute>();

					// Get variant IDs
					var nerfBarPowderLeft = context.ProductVariants.First(v => v.SKU == "PROD-1001");
					var nerfBarPowderRight = context.ProductVariants.First(v => v.SKU == "PROD-1002");
					var nerfBarStainlessLeft = context.ProductVariants.First(v => v.SKU == "PROD-1003");
					var nerfBarStainlessRight = context.ProductVariants.First(v => v.SKU == "PROD-1004");

					var wheelBlack10 = context.ProductVariants.First(v => v.SKU == "PROD-2001");
					var wheelBlack12 = context.ProductVariants.First(v => v.SKU == "PROD-2002");
					var wheelRed10 = context.ProductVariants.First(v => v.SKU == "PROD-2003");
					var wheelRed12 = context.ProductVariants.First(v => v.SKU == "PROD-2004");

					var floorPanKit = context.ProductVariants.First(v => v.SKU == "PRC-1130280");
					var cleaningSpray = context.ProductVariants.First(v => v.SKU == "PROD-5001");

					// Nerf Bar - Powder Coated Left Attributes
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderLeft.VariantID, AttributeName = "Material", AttributeValue = "Powder Coated", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderLeft.VariantID, AttributeName = "Side", AttributeValue = "Left", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderLeft.VariantID, AttributeName = "Finish", AttributeValue = "Matte Black", IsVariantAttribute = false, DisplayOrder = 3 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderLeft.VariantID, AttributeName = "Installation", AttributeValue = "Bolt-on", IsVariantAttribute = false, DisplayOrder = 4 });

					// Nerf Bar - Powder Coated Right
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderRight.VariantID, AttributeName = "Material", AttributeValue = "Powder Coated", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderRight.VariantID, AttributeName = "Side", AttributeValue = "Right", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderRight.VariantID, AttributeName = "Finish", AttributeValue = "Matte Black", IsVariantAttribute = false, DisplayOrder = 3 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarPowderRight.VariantID, AttributeName = "Installation", AttributeValue = "Bolt-on", IsVariantAttribute = false, DisplayOrder = 4 });

					// Nerf Bar - Stainless Steel Left
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessLeft.VariantID, AttributeName = "Material", AttributeValue = "Stainless Steel", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessLeft.VariantID, AttributeName = "Side", AttributeValue = "Left", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessLeft.VariantID, AttributeName = "Finish", AttributeValue = "Polished", IsVariantAttribute = false, DisplayOrder = 3 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessLeft.VariantID, AttributeName = "Installation", AttributeValue = "Bolt-on", IsVariantAttribute = false, DisplayOrder = 4 });

					// Nerf Bar - Stainless Steel Right
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessRight.VariantID, AttributeName = "Material", AttributeValue = "Stainless Steel", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessRight.VariantID, AttributeName = "Side", AttributeValue = "Right", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessRight.VariantID, AttributeName = "Finish", AttributeValue = "Polished", IsVariantAttribute = false, DisplayOrder = 3 });
					attributes.Add(new ProductAttribute { VariantID = nerfBarStainlessRight.VariantID, AttributeName = "Installation", AttributeValue = "Bolt-on", IsVariantAttribute = false, DisplayOrder = 4 });

					// Steering Wheel - Black 10 inch
					attributes.Add(new ProductAttribute { VariantID = wheelBlack10.VariantID, AttributeName = "Color", AttributeValue = "Black", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = wheelBlack10.VariantID, AttributeName = "Diameter", AttributeValue = "10 inch", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = wheelBlack10.VariantID, AttributeName = "Grip Type", AttributeValue = "Rubber", IsVariantAttribute = false, DisplayOrder = 3 });

					// Steering Wheel - Black 12 inch
					attributes.Add(new ProductAttribute { VariantID = wheelBlack12.VariantID, AttributeName = "Color", AttributeValue = "Black", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = wheelBlack12.VariantID, AttributeName = "Diameter", AttributeValue = "12 inch", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = wheelBlack12.VariantID, AttributeName = "Grip Type", AttributeValue = "Rubber", IsVariantAttribute = false, DisplayOrder = 3 });

					// Steering Wheel - Red 10 inch
					attributes.Add(new ProductAttribute { VariantID = wheelRed10.VariantID, AttributeName = "Color", AttributeValue = "Red", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = wheelRed10.VariantID, AttributeName = "Diameter", AttributeValue = "10 inch", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = wheelRed10.VariantID, AttributeName = "Grip Type", AttributeValue = "Rubber", IsVariantAttribute = false, DisplayOrder = 3 });

					// Steering Wheel - Red 12 inch
					attributes.Add(new ProductAttribute { VariantID = wheelRed12.VariantID, AttributeName = "Color", AttributeValue = "Red", IsVariantAttribute = true, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = wheelRed12.VariantID, AttributeName = "Diameter", AttributeValue = "12 inch", IsVariantAttribute = true, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = wheelRed12.VariantID, AttributeName = "Grip Type", AttributeValue = "Rubber", IsVariantAttribute = false, DisplayOrder = 3 });

					// Floor Pan Bolt Kit (Standalone)
					attributes.Add(new ProductAttribute { VariantID = floorPanKit.VariantID, AttributeName = "Kit Contents", AttributeValue = "10 bolts, 10 washers, 10 nuts", IsVariantAttribute = false, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = floorPanKit.VariantID, AttributeName = "Material", AttributeValue = "Steel", IsVariantAttribute = false, DisplayOrder = 2 });
					attributes.Add(new ProductAttribute { VariantID = floorPanKit.VariantID, AttributeName = "Finish", AttributeValue = "Zinc Plated", IsVariantAttribute = false, DisplayOrder = 3 });

					// Chassis Cleaning Spray (Standalone)
					attributes.Add(new ProductAttribute { VariantID = cleaningSpray.VariantID, AttributeName = "Volume", AttributeValue = "500ml", IsVariantAttribute = false, DisplayOrder = 1 });
					attributes.Add(new ProductAttribute { VariantID = cleaningSpray.VariantID, AttributeName = "Type", AttributeValue = "Biodegradable", IsVariantAttribute = false, DisplayOrder = 2 });

					context.ProductAttributes.AddRange(attributes);
					context.SaveChanges();
					}
				}
			}
		}
	}