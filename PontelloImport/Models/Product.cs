//using System.ComponentModel.DataAnnotations;

//namespace PontelloImport.Models
//{
//    public class Product
//    {
//        [Key]
//        public int ID { get; set; }
//        public string? Handle { get; set; }
//        public string? Title { get; set; }
//        public string? Description { get; set; }
//        public string? SKU { get; set; }
//        public string? Name { get; set; }
//        public double Price { get; set; }
//        public int InventoryQuantity { get; set; }
//        public string? Type { get; set; }


//    }
//}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
	{
	public class Product : Auditable
		{
		// ===== PRIMARY KEY =====

		[Key]
		public int ProductID { get; set; }

		// ===== AUTO-GENERATED FIELDS =====

		//[Required]
		[StringLength(100)]
		[Display(Name = "Handle")]
		public string Handle { get; set; } = "";  // Auto-generated from Title

		//[Required]
		[StringLength(50)]
		public string SKU { get; set; } = "";  // Auto-generated from BaseCode + sequence

		// ===== REQUIRED FIELDS =====

		[Required(ErrorMessage = "Product title is required")]
		[StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
		[Display(Name = "Product Title")]
		public string Title { get; set; } = "";

		[Required(ErrorMessage = "Price is required")]
		[Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
		[DataType(DataType.Currency)]
		[Column(TypeName = "decimal(10,2)")]
		public decimal Price { get; set; }

		// ===== FOREIGN KEYS =====

		[Required(ErrorMessage = "Vendor is required")]
		[Display(Name = "Vendor")]
		public int VendorID { get; set; }
		public Vendor? Vendor { get; set; }

		[Required(ErrorMessage = "Category is required")]
		[Display(Name = "Category")]
		public int ProductCategoryID { get; set; }
		public ProductCategory? ProductCategory { get; set; }

		// ===== OPTIONAL FIELDS =====

		[Display(Name = "Description")]
		public string? BodyHTML { get; set; }

		[StringLength(100)]
		[Display(Name = "Type")]
		public string? Type { get; set; }

		[StringLength(500)]
		[Display(Name = "Tags")]
		public string? Tags { get; set; }

		[DataType(DataType.Currency)]
		[Column(TypeName = "decimal(10,2)")]
		[Display(Name = "Compare At Price")]
		public decimal? CompareAtPrice { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
		[Display(Name = "Quantity")]
		public int InventoryQuantity { get; set; } = 0;

		[StringLength(20)]
		[Display(Name = "Inventory Policy")]
		public string InventoryPolicy { get; set; } = "deny";  // "deny" or "continue"

		[Display(Name = "Requires Shipping")]
		public bool RequiresShipping { get; set; } = true;

		[Display(Name = "Taxable")]
		public bool IsTaxable { get; set; } = true;

		[StringLength(100)]
		[Display(Name = "Barcode")]
		public string? Barcode { get; set; }

		[Column(TypeName = "decimal(10,2)")]
		[Display(Name = "Weight (g)")]
		public decimal? Weight { get; set; }

		// ===== VARIANT FIELDS =====

		[StringLength(50)]
		[Display(Name = "Variant Attribute 1")]
		public string? Option1Name { get; set; }

		[StringLength(50)]
		[Display(Name = "Variant Value 1")]
		public string? Option1Value { get; set; }

		[StringLength(50)]
		[Display(Name = "Variant Attribute 2")]
		public string? Option2Name { get; set; }

		[StringLength(50)]
		[Display(Name = "Variant Value 2")]
		public string? Option2Value { get; set; }

		[StringLength(50)]
		[Display(Name = "Variant Attribute 3")]
		public string? Option3Name { get; set; }

		[StringLength(50)]
		[Display(Name = "Variant Value 3")]
		public string? Option3Value { get; set; }

		[StringLength(100)]
		[Display(Name = "Parent Product Handle")]
		public string? ParentProductHandle { get; set; }

		// ===== STATUS =====

		[Display(Name = "Active")]
		public bool IsActive { get; set; } = true;

		// ===== COMPUTED PROPERTIES (Not Mapped to DB) =====

		[NotMapped]
		public bool HasVariants => !string.IsNullOrEmpty(Option1Name);

		[NotMapped]
		public string VariantSummary
			{
			get
				{
				var parts = new List<string>();
				if (!string.IsNullOrEmpty(Option1Value)) parts.Add(Option1Value);
				if (!string.IsNullOrEmpty(Option2Value)) parts.Add(Option2Value);
				if (!string.IsNullOrEmpty(Option3Value)) parts.Add(Option3Value);
				return string.Join(" / ", parts);
				}
			}
		}
	}
