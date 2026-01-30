using Microsoft.EntityFrameworkCore;
using PontelloImport.Models;

//namespace PontelloImport.Data
//{
//    public class PontelloDbContext : DbContext
//    {
//        public PontelloDbContext(DbContextOptions<PontelloDbContext> options)
//            : base(options)
//        {
//        }
//        public DbSet<Product> Product { get; set; }
//    }

//}

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
		public DbSet<Vendor> Vendors { get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }

		// ===== MODEL CONFIGURATION =====

		protected override void OnModelCreating(ModelBuilder modelBuilder)
			{
			base.OnModelCreating(modelBuilder);

			// ----- PRODUCT CONFIGURATION -----

			// Unique indexes
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.Handle)
				.IsUnique();

			modelBuilder.Entity<Product>()
				.HasIndex(p => p.SKU)
				.IsUnique();

			// Index for ParentProductHandle (variant grouping)
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.ParentProductHandle);

			// Index for active products
			modelBuilder.Entity<Product>()
				.HasIndex(p => p.IsActive);

			// Relationships - Prevent cascade delete
			modelBuilder.Entity<Product>()
				.HasOne(p => p.Vendor)
				.WithMany(v => v.Products)
				.HasForeignKey(p => p.VendorID)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Product>()
				.HasOne(p => p.ProductCategory)
				.WithMany(c => c.Products)
				.HasForeignKey(p => p.ProductCategoryID)
				.OnDelete(DeleteBehavior.Restrict);

			// ----- VENDOR CONFIGURATION -----

			modelBuilder.Entity<Vendor>()
				.HasIndex(v => v.VendorName)
				.IsUnique();

			modelBuilder.Entity<Vendor>()
				.HasIndex(v => v.VendorSlug)
				.IsUnique();

			// ----- PRODUCTCATEGORY CONFIGURATION -----

			modelBuilder.Entity<ProductCategory>()
				.HasIndex(c => c.CategoryName)
				.IsUnique();

			modelBuilder.Entity<ProductCategory>()
				.HasIndex(c => c.CategorySlug)
				.IsUnique();

			// Self-referencing relationship (Parent/Child categories)
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
