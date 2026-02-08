using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public class Product : Auditable
		{
		[Key]
		public int ProductID { get; set; }

		[Required(ErrorMessage = "Product title is required")]
		[StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
		[Display(Name = "Product Title")]
		public string Title { get; set; }

		
		[StringLength(255)]
		[Display(Name = "URL Handle")]
		public string Handle { get; set; }

		[Display(Name = "Vendor")]
		public int? VendorID { get; set; }
		public Vendor? Vendor { get; set; }

		[Display(Name = "Category")]
		public int? ProductCategoryID { get; set; }
		public ProductCategory? ProductCategory { get; set; }

		[Display(Name = "Description")]
		[DataType(DataType.MultilineText)]
		public string? Description { get; set; }

		[StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
		[Display(Name = "Product Type")]
		public string? Type { get; set; }

		[Display(Name = "Tags")]
		public string? Tags { get; set; }

		[Display(Name = "Status")]
		public ProductStatus Status { get; set; } = ProductStatus.Draft;

		[Display(Name = "Active")]
		public bool IsActive { get; set; } = true;

		// Navigation property
		public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
		}
	}