using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class ProductVariant
    {
        public int VariantID { get; set; }

        public int ProductID { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(100)]
        public string SKU { get; set; }

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CompareAtPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostPrice { get; set; }

        public int InventoryQuantity { get; set; } = 0;

        [MaxLength(20)]
        public string InventoryPolicy { get; set; } = "deny";
        // Values: "deny" / "continue" / "special_order"

        [MaxLength(20)]
        public string StockPolicy { get; set; } = "deny";
        // "deny" = do not allow order if stock <= 0
        // "allow" = allow backorders (stock can go negative)

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; set; }

        [MaxLength(10)]
        public string WeightUnit { get; set; } = "lb";

        public bool RequiresShipping { get; set; } = true;
        public bool IsTaxable { get; set; } = true;

        // Option columns — replaces old ProductAttribute table
        [MaxLength(100)] public string? Option1Name { get; set; }
        [MaxLength(100)] public string? Option1Value { get; set; }
        [MaxLength(100)] public string? Option2Name { get; set; }
        [MaxLength(100)] public string? Option2Value { get; set; }
        [MaxLength(100)] public string? Option3Name { get; set; }
        [MaxLength(100)] public string? Option3Value { get; set; }
        [MaxLength(100)] public string? Option4Name { get; set; }
        [MaxLength(100)] public string? Option4Value { get; set; }
        [MaxLength(100)] public string? Option5Name { get; set; }
        [MaxLength(100)] public string? Option5Value { get; set; }

        public string? VariantImageUrl { get; set; }

        public bool IsDefault { get; set; } = false;
        // True for standalone "Default Title" variants

        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
