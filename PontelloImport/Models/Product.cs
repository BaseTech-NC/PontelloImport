using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class Product
    {
        public int ProductID { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required, MaxLength(100)]
        public string Handle { get; set; }

        public int VendorID { get; set; }
        public Vendor? Vendor { get; set; }

        public int ProductCategoryID { get; set; }
        public ProductCategory? ProductCategory { get; set; }

        public int? ProductTypeID { get; set; }
        public ProductType? ProductType { get; set; }

        public string? Description { get; set; }

        public string? Tags { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductSpecification> ProductSpecifications { get; set; } = new List<ProductSpecification>();
    }
}
