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
							CategoryName = "Cleaning Products",
							CategorySlug = "cleaning-products",
							CategoryDescription = "Kart cleaning and maintenance products",
							DisplayOrder = 5,
							IsActive = true
						},
						new ProductCategory
						{
							CategoryName = "Safety Equipment",
							CategorySlug = "safety-equipment",
							CategoryDescription = "Helmets, suits, gloves, and safety gear",
							DisplayOrder = 6,
							IsActive = true
						}
					};

					context.ProductCategories.AddRange(categories);
					context.SaveChanges();
					}
				}
			}
		}
	}
