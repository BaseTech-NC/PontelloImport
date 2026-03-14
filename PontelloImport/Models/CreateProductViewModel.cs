using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public class CreateProductViewModel
		{

		public Product Product { get; set; } = new Product();
		// All ProductVariant fields
		public ProductVariant Variant { get; set; } = new ProductVariant();

		// List of attributes to create
		public List<AttributeInputModel> Attributes { get; set; } = new List<AttributeInputModel>();
		}

	// Helper class for attribute input
	public class AttributeInputModel
		{
		public string AttributeName { get; set; } = string.Empty;
		public string AttributeValue { get; set; } = string.Empty;
		public bool IsVariantAttribute { get; set; } = false;
		public int DisplayOrder { get; set; } = 0;
		}

	// Simple flat model for the quick-create form
	public class QuickCreateViewModel
		{
		[Required(ErrorMessage = "Product title is required.")]
		[MaxLength(255)]
		public string ProductTitle { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please select a vendor.")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a vendor.")]
		public int VendorID { get; set; }

		[Required(ErrorMessage = "Please select a category.")]
		[Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
		public int ProductCategoryID { get; set; }

		public int? ProductTypeID { get; set; }

		public ProductStatus Status { get; set; } = ProductStatus.Draft;

		[Required(ErrorMessage = "SKU is required.")]
		[MaxLength(100)]
		public string SKU { get; set; } = string.Empty;

		[Required(ErrorMessage = "Price is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
		public decimal Price { get; set; }

		[Required(ErrorMessage = "Inventory quantity is required.")]
		[Range(0, int.MaxValue, ErrorMessage = "Inventory quantity cannot be negative.")]
		public int InventoryQuantity { get; set; } = 0;

		[MaxLength(100)]
		public string? Option1Name { get; set; }

		[MaxLength(100)]
		public string? Option1Value { get; set; }
		}
	}