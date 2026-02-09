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


		[Required(ErrorMessage = "Product Title is required for product creation.")]
		[StringLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
		[Display(Name = "Product Title")]
		public string Title { get; set; }


		[StringLength(255)]
		[Display(Name = "URL Handle")]
		public string Handle { get; set; }

		[Required(ErrorMessage = "SKU is required for product creation.")]
		[StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters.")]
		[RegularExpression(@"^[A-Z]{2,4}-\d{3,7}(-[A-Z]{1,3})?$",
			ErrorMessage = "SKU must follow format: AB-123 or ABC-1234-XY (letters-numbers or letters-numbers-letters)")]
		[Display(Name = "SKU")]
		public string SKU { get; set; }

		[Required(ErrorMessage = "Price is required for product creation.")]
		[Column(TypeName = "decimal(10,2)")]
		[Range(0.01, 999999.99, ErrorMessage = "Price must be a positive value.")]
		[Display(Name = "Price")]
		[DataType(DataType.Currency)]
		public decimal Price { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		[Range(0.01, 999999.99, ErrorMessage = "Compare At Price must be a positive value.")]
		[Display(Name = "Compare At Price")]
		[DataType(DataType.Currency)]
		public decimal? CompareAtPrice { get; set; }

		[Required(ErrorMessage = "Inventory Quantity is required for product creation.")]
		[Range(0, int.MaxValue, ErrorMessage = "Inventory Quantity cannot be negative. Please enter 0 or a positive number.")]
		[Display(Name = "Inventory Quantity")]
		public int InventoryQuantity { get; set; } = 0;

		[StringLength(50)]
		[Display(Name = "Inventory Policy")]
		public string InventoryPolicy { get; set; } = "deny";

		[Range(0, int.MaxValue, ErrorMessage = "Weight cannot be negative. Please enter 0 or a positive number.")]
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