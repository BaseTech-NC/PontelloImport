using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class DealerApplication
    {
        public int ApplicationID { get; set; }

        [Required, MaxLength(255)]
        public string SubmittedCompanyName { get; set; }

        [Required, MaxLength(100)]
        public string SubmittedContactName { get; set; }

        [Required, MaxLength(20)]
        public string SubmittedContactPhone { get; set; }

        [Required, MaxLength(100)]
        public string SubmittedEmail { get; set; }

        public int SubmittedAddressID { get; set; }
        public Address? SubmittedAddress { get; set; }

        [MaxLength(50)]
        public string? BusinessNumber { get; set; }

        public int? RequestedPaymentTermsID { get; set; }
        public PaymentTerms? RequestedPaymentTerms { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewNotes { get; set; }

        public int? ApprovedDealerID { get; set; }
        public Dealer? ApprovedDealer { get; set; }

        public int? DealerID { get; set; }
        public Dealer? Dealer { get; set; }
    }
}
