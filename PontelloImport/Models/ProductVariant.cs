using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
	{
	public class ProductVariant : Auditable
		{
		[Key]
		public int VariantID { get; set; }

		[Display(Name = "Parent Product")]
		public int? ProductID { get; set; }
		public Product? Product { get; set; }


		[Required(ErrorMessage = "Product title is required")]
		[StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
		[Display(Name = "Product Title")]
		public string Title { get; set; }

		
		[StringLength(255)]
		[Display(Name = "URL Handle")]
		public string Handle { get; set; }

		[Required(ErrorMessage = "SKU is required")]
		[StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
		[Display(Name = "SKU")]
		public string SKU { get; set; }

		[Required(ErrorMessage = "Price is required")]
		[Column(TypeName = "decimal(10,2)")]
		[Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
		[Display(Name = "Price")]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		[Range(0.01, 999999.99, ErrorMessage = "Compare at price must be between $0.01 and $999,999.99")]
		[Display(Name = "Compare At Price")]
		[DataType(DataType.Currency)]
		public decimal? CompareAtPrice { get; set; }

		[Required(ErrorMessage = "Inventory quantity is required")]
		[Range(0, int.MaxValue, ErrorMessage = "Inventory quantity cannot be negative")]
		[Display(Name = "Inventory Quantity")]
		public int InventoryQuantity { get; set; } = 0;

		[Required(ErrorMessage = "Inventory policy is required")]
		[StringLength(50)]
		[Display(Name = "Inventory Policy")]
		public string InventoryPolicy { get; set; } = "deny";

		[Range(0, int.MaxValue, ErrorMessage = "Weight cannot be negative")]
		[Display(Name = "Weight")]
		public int? Weight { get; set; }

		[StringLength(100, ErrorMessage = "Barcode cannot exceed 100 characters")]
		[Display(Name = "Barcode")]
		public string? Barcode { get; set; }

		[Display(Name = "Requires Shipping")]
		public bool RequiresShipping { get; set; } = true;

		[Display(Name = "Taxable")]
		public bool IsTaxable { get; set; } = true;

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
		public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
		}
	}