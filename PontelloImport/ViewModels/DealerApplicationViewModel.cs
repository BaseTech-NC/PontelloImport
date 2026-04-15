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

        [Required]
        [RegularExpression(@"^[\d\s\-\(\)\+]{7,20}$",
            ErrorMessage = "Enter a valid phone number")]
        [MaxLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = "";

        [MaxLength(255)]
        [Display(Name = "Company Name")]
        public string? Company { get; set; }

        [MaxLength(255)]
        [Display(Name = "Business Address")]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(50)]
        [Display(Name = "Province / State")]
        public string? ProvinceState { get; set; }

        [MaxLength(20)]
        [Display(Name = "Postal Code / Zip Code")]
        public string? PostalZipCode { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Brief description of your company")]
        public string? CompanyDescription { get; set; }

        [MaxLength(255)]
        [Display(Name = "Website / Social Media")]
        public string? WebsiteSocialMedia { get; set; }
    }
}
