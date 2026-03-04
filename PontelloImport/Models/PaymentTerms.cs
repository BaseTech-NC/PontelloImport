using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class PaymentTerms
    {
        public int PaymentTermsID { get; set; }

        [Required, MaxLength(100)]
        public string TermName { get; set; }

        [Required, MaxLength(20)]
        public string TermCode { get; set; }

        [MaxLength(255)]
        public string? TermDescription { get; set; }

        public int DaysUntilDue { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Dealer> Dealers { get; set; } = new List<Dealer>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
