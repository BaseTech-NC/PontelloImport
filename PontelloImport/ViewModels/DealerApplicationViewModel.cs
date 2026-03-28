using System.ComponentModel.DataAnnotations;

namespace PontelloImport.ViewModels
{
    public class DealerApplicationViewModel
    {
        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = "";

        [Required, EmailAddress, MaxLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = "";

        [Required, Phone, MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = "";

        [Required, MaxLength(255)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = "";

        [Required, MaxLength(255)]
        [Display(Name = "Business Address")]
        public string BusinessAddress { get; set; } = "";

        [Required, MaxLength(100)]
        public string City { get; set; } = "";

        [Required, MaxLength(50)]
        public string Province { get; set; } = "";

        [Required, MaxLength(10)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = "";

        [MaxLength(50)]
        [Display(Name = "Business Number (GST/HST)")]
        public string? BusinessNumber { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Additional Notes")]
        public string? Notes { get; set; }
    }
}
