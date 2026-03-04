using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class ProductCategory
    {
        public int CategoryID { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; }

        [Required, MaxLength(100)]
        public string CategorySlug { get; set; }

        public string? CategoryDescription { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public int? ParentCategoryID { get; set; }
        public ProductCategory? ParentCategory { get; set; }
        public ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();

        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
