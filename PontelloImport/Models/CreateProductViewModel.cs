namespace PontelloImport.Models
	{
	public class CreateProductViewModel
		{
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
	}