using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class Vendor
    {
        public int VendorID { get; set; }

        [Required, MaxLength(255)]
        public string VendorName { get; set; }

        [Required, MaxLength(100)]
        public string VendorSlug { get; set; }

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(255)]
        public string? Website { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
