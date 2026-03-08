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

		public DbSet<Product> Products { get; set; }
		public DbSet<ProductVariant> ProductVariants { get; set; }
		public DbSet<ProductAttribute> ProductAttributes { get; set; }
		public DbSet<Vendor> Vendors { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<OrderHistory> OrderHistory { get; set; }

        // ===== MODEL CONFIGURATION =====

        protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
			base.OnModelCreating(modelBuilder);

			// ============================================================
			// PRODUCT INDEXES
			// ============================================================

			// Unique handle for URL routing
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.Handle)
				.IsUnique();

			// Status for filtering (Draft/Published/Archived)
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.Status);

			// IsActive for visibility filtering
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.IsActive);

			// Composite index for status + active queries
			modelBuilder.Entity<Product>()
				.HasIndex(p => new { p.Status, p.IsActive });

			// ============================================================
			// PRODUCT VARIANT INDEXES
			// ============================================================

			// Unique handle for URL routing
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.Handle)
				.IsUnique();

			// Unique SKU for product identification
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.SKU)
				.IsUnique();

			// ProductID for parent lookups
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.ProductID);

			// Status for filtering (Draft/Published/Archived)
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.Status);

			// IsActive for visibility filtering
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.IsActive);

			// Price for sorting/filtering
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.Price);

			// Weight for sorting/filtering
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => v.Weight);

			// Composite index for parent + active queries
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => new { v.ProductID, v.IsActive });

			// Composite index for status + active queries
			modelBuilder.Entity<ProductVariant>()
				.HasIndex(v => new { v.Status, v.IsActive });

			// ============================================================
			// PRODUCT ATTRIBUTE INDEXES
			// ============================================================

			modelBuilder.Entity<ProductAttribute>()
				.HasIndex(a => a.VariantID);

			modelBuilder.Entity<ProductAttribute>()
				.HasIndex(a => a.AttributeName);

			modelBuilder.Entity<ProductAttribute>()
				.HasIndex(a => new { a.VariantID, a.IsVariantAttribute });

			modelBuilder.Entity<ProductAttribute>()
				.HasIndex(a => new { a.AttributeName, a.AttributeValue });

			// ============================================================
			// VENDOR INDEXES
			// ============================================================

			modelBuilder.Entity<Vendor>()
				.HasIndex(v => v.VendorName)
				.IsUnique();

			modelBuilder.Entity<Vendor>()
				.HasIndex(v => v.VendorSlug)
				.IsUnique();

			// ============================================================
			// PRODUCT CATEGORY INDEXES
			// ============================================================

			modelBuilder.Entity<ProductCategory>()
				.HasIndex(c => c.CategoryName)
				.IsUnique();

			modelBuilder.Entity<ProductCategory>()
				.HasIndex(c => c.CategorySlug)
				.IsUnique();

			// ============================================================
			// RELATIONSHIPS
			// ============================================================

			// Product -> Vendor (Restrict delete if products exist)
			modelBuilder.Entity<Product>()
				.HasOne(p => p.Vendor)
				.WithMany(v => v.Products)
				.HasForeignKey(p => p.VendorID)
				.OnDelete(DeleteBehavior.Restrict);

			// Product -> Category (Restrict delete if products exist)
			modelBuilder.Entity<Product>()
				.HasOne(p => p.ProductCategory)
				.WithMany(c => c.Products)
				.HasForeignKey(p => p.ProductCategoryID)
				.OnDelete(DeleteBehavior.Restrict);

			// ProductVariant -> Product (Restrict delete if variants exist)
			modelBuilder.Entity<ProductVariant>()
				.HasOne(v => v.Product)
				.WithMany(p => p.Variants)
				.HasForeignKey(v => v.ProductID)
				.OnDelete(DeleteBehavior.Restrict);

			// ProductAttribute -> ProductVariant (Cascade delete when variant deleted)
			modelBuilder.Entity<ProductAttribute>()
				.HasOne(a => a.Variant)
				.WithMany(v => v.Attributes)
				.HasForeignKey(a => a.VariantID)
				.OnDelete(DeleteBehavior.Cascade);

			// ProductCategory self-referencing (Parent/Child categories)
			modelBuilder.Entity<ProductCategory>()
				.HasOne(c => c.ParentCategory)
				.WithMany(c => c.ChildCategories)
				.HasForeignKey(c => c.ParentCategoryID)
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