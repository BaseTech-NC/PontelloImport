using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class ProductType
    {
        public int ProductTypeID { get; set; }

        [Required, MaxLength(100)]
        public string TypeName { get; set; }

        [Required, MaxLength(100)]
        public string TypeSlug { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
