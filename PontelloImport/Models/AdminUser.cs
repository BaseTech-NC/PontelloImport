using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class AdminUser
    {
        public int AdminUserID { get; set; }

        [Required]
        public string ApplicationUserID { get; set; }
        // [NotMapped] because ApplicationUser lives in ApplicationDbContext
        [NotMapped] public ApplicationUser? ApplicationUser { get; set; }

        public bool IsSuperAdmin { get; set; } = false;
    }
}
