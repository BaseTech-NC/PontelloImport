using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public class ProductCategory : Auditable
		{
		// ===== PRIMARY KEY =====
		[Key]
		public int CategoryID { get; set; }

		// ===== REQUIRED FIELDS =====

		[Required(ErrorMessage = "Category name is required")]
		[StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
		[Display(Name = "Category Name")]
		public string CategoryName { get; set; } = "";

		// ===== AUTO-GENERATED FIELDS =====

		[StringLength(100)]
		[Display(Name = "Slug")]
		public string CategorySlug { get; set; } = "";  // Auto-generated from CategoryName

		// ===== OPTIONAL FIELDS =====

		[Display(Name = "Description")]
		public string? CategoryDescription { get; set; }

		[Display(Name = "Display Order")]
		public int DisplayOrder { get; set; } = 0;

		// ===== SELF-REFERENCE (Hierarchy) =====

		[Display(Name = "Parent Category")]
		public int? ParentCategoryID { get; set; }
		public ProductCategory? ParentCategory { get; set; }

		// ===== STATUS =====

		[Display(Name = "Active")]
		public bool IsActive { get; set; } = true;

		// ===== NAVIGATION PROPERTIES =====

		// Child categories (subcategories)
		public ICollection<ProductCategory> ChildCategories { get; set; } = new HashSet<ProductCategory>();

		// Products in this category
		// Uncomment after Product model is updated in Chunk 4
		public ICollection<Product> Products { get; set; } = new HashSet<Product>();
		}
	}