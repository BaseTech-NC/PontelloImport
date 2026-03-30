using Microsoft.EntityFrameworkCore;
using PontelloImport.Models;

namespace PontelloImport.Data
	{
	public class PontelloDbContext : DbContext
		{
		// ===== AUDIT TRACKING =====
		private readonly IHttpContextAccessor? _httpContextAccessor;
		public int? CurrentUserId { get; private set; }

		// ===== CONSTRUCTORS =====

		// Constructor for runtime (with HttpContext)
		public PontelloDbContext(
			DbContextOptions<PontelloDbContext> options,
			IHttpContextAccessor httpContextAccessor)
			: base(options)
			{
			_httpContextAccessor = httpContextAccessor;

			if (_httpContextAccessor?.HttpContext != null)
				{
				// Get UserID from claims (set during login)
				var userIdClaim = _httpContextAccessor.HttpContext
					.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

				if (int.TryParse(userIdClaim, out int userId))
					{
					CurrentUserId = userId;
					}
				}
			}

		// Constructor for seeding/migrations (no HttpContext)
		public PontelloDbContext(DbContextOptions<PontelloDbContext> options)
			: base(options)
			{
			_httpContextAccessor = null;
			CurrentUserId = null;
			}

		// ===== DBSETS =====

		// DbSets — Product Management
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductVariant> ProductVariants { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }
		public DbSet<ProductType> ProductTypes { get; set; }
		public DbSet<ProductSpecification> ProductSpecifications { get; set; }
		public DbSet<Vendor> Vendors { get; set; }
		public DbSet<OptionTemplate> OptionTemplates { get; set; }

		// DbSets — User Management
		public DbSet<AdminUser> AdminUsers { get; set; }
		public DbSet<Dealer> Dealers { get; set; }
		public DbSet<DealerApplication> DealerApplications { get; set; }
		public DbSet<Address> Addresses { get; set; }
		public DbSet<PaymentTerms> PaymentTerms { get; set; }

		// DbSets — Order Management
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderLine> OrderLines { get; set; }
		public DbSet<OrderHistory> OrderHistories { get; set; }
		public DbSet<Cart> Carts { get; set; }
		public DbSet<CartItem> CartItems { get; set; }
		public DbSet<OrderSequence> OrderSequence { get; set; }

		// ===== MODEL CONFIGURATION =====

		protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Vendor>().HasIndex(v => v.VendorName).IsUnique();
			modelBuilder.Entity<Vendor>().HasIndex(v => v.VendorSlug).IsUnique();

			modelBuilder.Entity<ProductCategory>().HasIndex(c => c.CategoryName).IsUnique();
			modelBuilder.Entity<ProductCategory>().HasIndex(c => c.CategorySlug).IsUnique();

			modelBuilder.Entity<Product>().HasIndex(p => p.Handle).IsUnique();

			modelBuilder.Entity<ProductVariant>().HasIndex(v => v.SKU).IsUnique();

			modelBuilder.Entity<PaymentTerms>().HasIndex(pt => pt.TermName).IsUnique();
			modelBuilder.Entity<PaymentTerms>().HasIndex(pt => pt.TermCode).IsUnique();

			modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

			modelBuilder.Entity<Cart>().HasIndex(c => c.DealerID).IsUnique();

			modelBuilder.Entity<AdminUser>().HasIndex(a => a.ApplicationUserID).IsUnique();

			modelBuilder.Entity<Dealer>().HasIndex(d => d.CompanyName).IsUnique();
			modelBuilder.Entity<Dealer>().HasIndex(d => d.ApplicationUserID).IsUnique();

			modelBuilder.Entity<ProductCategory>()
				.HasOne(c => c.ParentCategory)
				.WithMany(c => c.SubCategories)
				.HasForeignKey(c => c.ParentCategoryID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Order>()
				.HasOne(o => o.RootOrder)
				.WithMany()
				.HasForeignKey(o => o.RootOrderID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Order>()
				.HasOne(o => o.PreviousOrder)
				.WithMany()
				.HasForeignKey(o => o.PreviousOrderID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Dealer>()
				.HasOne(d => d.BillingAddress)
				.WithMany()
				.HasForeignKey(d => d.BillingAddressID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Dealer>()
				.HasOne(d => d.ShippingAddress)
				.WithMany()
				.HasForeignKey(d => d.ShippingAddressID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<OrderLine>()
				.HasOne(ol => ol.Order)
				.WithMany(o => o.OrderLines)
				.HasForeignKey(ol => ol.OrderID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<CartItem>()
				.HasOne(ci => ci.Cart)
				.WithMany(c => c.CartItems)
				.HasForeignKey(ci => ci.CartID)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<Order>()
				.Ignore(o => o.PONumber);

			// Non-conventional PK names require explicit HasKey
			modelBuilder.Entity<ProductVariant>().HasKey(v => v.VariantID);
			modelBuilder.Entity<ProductCategory>().HasKey(c => c.CategoryID);
			modelBuilder.Entity<ProductSpecification>().HasKey(ps => ps.SpecificationID);
			modelBuilder.Entity<DealerApplication>().HasKey(da => da.ApplicationID);
			modelBuilder.Entity<OrderHistory>().HasKey(oh => oh.HistoryID);

			// DealerApplication has two FKs to Dealer; configure each explicitly
			modelBuilder.Entity<DealerApplication>()
				.HasOne(da => da.Dealer)
				.WithMany(d => d.DealerApplications)
				.HasForeignKey(da => da.DealerID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<DealerApplication>()
				.HasOne(da => da.ApprovedDealer)
				.WithMany()
				.HasForeignKey(da => da.ApprovedDealerID)
				.OnDelete(DeleteBehavior.Restrict);

			}

		// ===== AUTOMATIC AUDIT TRACKING =====

		public override int SaveChanges(bool acceptAllChangesOnSuccess)
			{
			OnBeforeSaving();
			return base.SaveChanges(acceptAllChangesOnSuccess);
			}

		public override Task<int> SaveChangesAsync(
			bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = default)
			{
			OnBeforeSaving();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
			}

		private void OnBeforeSaving()
			{
			var entries = ChangeTracker.Entries();
			var now = DateTime.UtcNow;

			foreach (var entry in entries)
				{
				if (entry.Entity is IAuditable auditable)
					{
					switch (entry.State)
						{
						case EntityState.Added:
						auditable.CreatedDate = now;
						auditable.CreatedBy = CurrentUserId;
						auditable.ModifiedDate = now;
						auditable.ModifiedBy = CurrentUserId;
						break;

						case EntityState.Modified:
						auditable.ModifiedDate = now;
						auditable.ModifiedBy = CurrentUserId;
						// Prevent overwriting CreatedBy/CreatedDate
						entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
						entry.Property(nameof(IAuditable.CreatedDate)).IsModified = false;
						break;
						}
					}
				}
			}
		}
	}