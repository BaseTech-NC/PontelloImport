using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class CreateProductViewModel
    {
        public Product Product { get; set; } = new Product();
        public ProductVariant Variant { get; set; } = new ProductVariant();
        public List<AttributeInputModel> Attributes { get; set; } = new List<AttributeInputModel>();
    }

    public class AttributeInputModel
    {
        public string AttributeName { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;
        public bool IsVariantAttribute { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }

    // Flat model for the quick-create form — supports both simple and multi-variant creation
    public class QuickCreateViewModel
    {
        [Required(ErrorMessage = "Product title is required.")]
        [MaxLength(255)]
        public string ProductTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a vendor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a vendor.")]
        public int VendorID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int ProductCategoryID { get; set; }

        public int? ProductTypeID { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        public decimal? CostPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Inventory quantity cannot be negative.")]
        public int InventoryQuantity { get; set; } = 0;

        public string StockPolicy { get; set; } = "deny";

        [MaxLength(100)]
        public string? Barcode { get; set; }  // Optional UPC/EAN — simple path only

        // Single-option for simple path (dropdown-based)
        [MaxLength(100)]
        public string? SimpleOptionName { get; set; }   // e.g. "Size"

        [MaxLength(100)]
        public string? SimpleOptionValue { get; set; }  // e.g. "Small"

        public bool HasVariants { get; set; } = false;

        // Option dimension names (product-level — same across all variants)
        [MaxLength(100)]
        public string? Option1Name { get; set; }

        [MaxLength(100)]
        public string? Option2Name { get; set; }

        [MaxLength(100)]
        public string? Option3Name { get; set; }

        // One row per generated variant combination
        public List<VariantRowViewModel> Variants { get; set; } = new List<VariantRowViewModel>();

        // Optional product specifications (Model Year, Material, Thread Size, etc.)
        public List<SpecificationRowViewModel> Specifications { get; set; } = new();
    }

    // One row in the generated variants table
    public class VariantRowViewModel
    {
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Barcode { get; set; }

        public decimal Price { get; set; }

        public decimal? CostPrice { get; set; }

        public int Qty { get; set; }

        public string StockPolicy { get; set; } = "deny";

        [MaxLength(100)]
        public string? Option1Value { get; set; }

        [MaxLength(100)]
        public string? Option2Value { get; set; }

        [MaxLength(100)]
        public string? Option3Value { get; set; }
    }

    // One row in the Specifications dynamic list
    public class SpecificationRowViewModel
    {
        public string Label { get; set; } = "";   // e.g. "Model Year"
        public string Value { get; set; } = "";   // e.g. "2023"
    }


    public class EditProductViewModel
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product title is required.")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a vendor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a vendor.")]
        public int VendorID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int ProductCategoryID { get; set; }

        public int? ProductTypeID { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        // Determines which section renders — round-trips via hidden input
        public bool IsSimpleProduct { get; set; }

        public int SimpleVariantID { get; set; }

        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? CostPrice { get; set; }

        public decimal? CompareAtPrice { get; set; }

        public int InventoryQuantity { get; set; }

        public string StockPolicy { get; set; } = "deny";

        public decimal? Weight { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(100)]
        public string? SimpleOptionName { get; set; }

        [MaxLength(100)]
        public string? SimpleOptionValue { get; set; }

        [MaxLength(100)]
        public string? Option1Name { get; set; }

        [MaxLength(100)]
        public string? Option2Name { get; set; }

        [MaxLength(100)]
        public string? Option3Name { get; set; }

        // Which variant is set as default — bound from radio button group
        public int DefaultVariantID { get; set; }

        public List<EditVariantRowViewModel> Variants { get; set; } = new();

        public List<EditSpecificationRowViewModel> Specifications { get; set; } = new();
    }

    public class EditVariantRowViewModel
    {
        public int VariantID { get; set; }

        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? CostPrice { get; set; }

        public int InventoryQuantity { get; set; }

        public string StockPolicy { get; set; } = "deny";

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [MaxLength(100)]
        public string? Option1Value { get; set; }

        [MaxLength(100)]
        public string? Option2Value { get; set; }

        [MaxLength(100)]
        public string? Option3Value { get; set; }

        // Set true when user clicks Archive on this row
        public bool IsArchived { get; set; }
    }

    public class EditSpecificationRowViewModel
    {
        public int SpecID { get; set; }          // 0 = new
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public int DisplayOrder { get; set; }
    }
}
