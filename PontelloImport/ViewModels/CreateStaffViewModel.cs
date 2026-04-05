using System.ComponentModel.DataAnnotations;

namespace PontelloImport.ViewModels
{
    public class CreateStaffViewModel
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
        [Display(Name = "Role")]
        public string Role { get; set; } = "Staff";
    }
}
