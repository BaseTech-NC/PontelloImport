using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        // Navigation — [NotMapped] because AdminUser/Dealer live in PontelloDbContext
        [NotMapped] public AdminUser? AdminUser { get; set; }
        [NotMapped] public Dealer? Dealer { get; set; }
    }
}
