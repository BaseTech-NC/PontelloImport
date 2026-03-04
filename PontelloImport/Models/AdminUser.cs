using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class AdminUser
    {
        public int AdminUserID { get; set; }

        [Required]
        public string ApplicationUserID { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public bool IsSuperAdmin { get; set; } = false;
    }
}
