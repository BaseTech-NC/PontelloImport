using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PontelloImport.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, MaxLength(20)]
        public string UserType { get; set; } = "Dealer";
        // Values: "SuperAdmin" / "Admin" / "Dealer"

        // Navigation
        public AdminUser? AdminUser { get; set; }
        public Dealer? Dealer { get; set; }
    }
}
