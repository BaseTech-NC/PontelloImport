using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class DealerApplication
    {
        public int ApplicationID { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";

        [Required, MaxLength(100)]
        public string Email { get; set; } = "";

        [Required, MaxLength(20)]
        public string Phone { get; set; } = "";

        [MaxLength(255)]
        public string? Company { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? ProvinceState { get; set; }

        [MaxLength(20)]
        public string? PostalZipCode { get; set; }

        [MaxLength(1000)]
        public string? CompanyDescription { get; set; }

        [MaxLength(255)]
        public string? WebsiteSocialMedia { get; set; }

        [MaxLength(255)]
        public string? SubmittedCompanyName { get; set; }

        [MaxLength(100)]
        public string? SubmittedContactName { get; set; }

        [MaxLength(20)]
        public string? SubmittedContactPhone { get; set; }

        [MaxLength(100)]
        public string? SubmittedEmail { get; set; }

        public int? SubmittedAddressID { get; set; }
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
