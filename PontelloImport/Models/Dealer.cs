using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class Dealer
    {
        public int DealerID { get; set; }

        public string? ApplicationUserID { get; set; }
        // [NotMapped] because ApplicationUser lives in ApplicationDbContext
        [NotMapped] public ApplicationUser? ApplicationUser { get; set; }

        [Required, MaxLength(255)]
        public string CompanyName { get; set; }

        [Required, MaxLength(20)]
        public string ContactPhone { get; set; }

        public int BillingAddressID { get; set; }
        public Address? BillingAddress { get; set; }

        public int? ShippingAddressID { get; set; }
        public Address? ShippingAddress { get; set; }

        [MaxLength(50)]
        public string? BusinessNumber { get; set; }

        public bool IsTaxExempt { get; set; } = false;

        public int PaymentTermsID { get; set; }
        public PaymentTerms? PaymentTerms { get; set; }

        public string? Notes { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public ICollection<DealerApplication> DealerApplications { get; set; } = new List<DealerApplication>();
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
