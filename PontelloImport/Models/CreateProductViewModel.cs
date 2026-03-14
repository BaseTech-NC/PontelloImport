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

        [Required(ErrorMessage = "Please select a vendor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a vendor.")]
        public int VendorID { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int ProductCategoryID { get; set; }

        public int? ProductTypeID { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        // ── Simple path (HasVariants = false) ──────────────────────────────
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Inventory quantity cannot be negative.")]
        public int InventoryQuantity { get; set; } = 0;

        // Single-option support (simple path only)
        [MaxLength(100)]
        public string? Option1Name { get; set; }

        [MaxLength(100)]
        public string? Option1Value { get; set; }

        // ── Multi-variant path (HasVariants = true) ─────────────────────────
        public bool HasVariants { get; set; } = false;

        // Option dimension names (product-level — same across all variants)
        [MaxLength(100)]
        public string? Option2Name { get; set; }

        [MaxLength(100)]
        public string? Option3Name { get; set; }

        // One row per generated variant combination
        public List<VariantRowViewModel> Variants { get; set; } = new List<VariantRowViewModel>();
    }

    // One row in the generated variants table
    public class VariantRowViewModel
    {
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Qty { get; set; }

        [MaxLength(100)]
        public string? Option1Value { get; set; }

        [MaxLength(100)]
        public string? Option2Value { get; set; }

        [MaxLength(100)]
        public string? Option3Value { get; set; }
    }
}
