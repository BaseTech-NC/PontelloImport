using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
	{
	public class ProductAttribute : Auditable
		{
		// ===== PRIMARY KEY =====
		[Key]
		public int AttributeID { get; set; }

		// ===== FOREIGN KEY =====
		[Required]
		[Display(Name = "Product Variant")]
		public int VariantID { get; set; }
		

		// ===== REQUIRED FIELDS =====
		[Required(ErrorMessage = "Attribute name is required")]
		[StringLength(100)]
		[Display(Name = "Attribute Name")]
		public string AttributeName { get; set; } = "";  // e.g., "Color", "Size", "Material"

		[Required(ErrorMessage = "Attribute value is required")]
		[StringLength(500)]
		[Display(Name = "Attribute Value")]
		public string AttributeValue { get; set; } = "";  // e.g., "Black", "10 inch", "Stainless"

		// ===== CLASSIFICATION =====
		[Display(Name = "Variant Attribute")]
		public bool IsVariantAttribute { get; set; } = false;  // true = customer selects, false = spec only

		[Display(Name = "Display Order")]
		public int DisplayOrder { get; set; } = 0;  // Order for UI display

		public ProductVariant Variant { get; set; }
		}


	}