using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class OrderHistory
    {
        public int HistoryID { get; set; }

        public int OrderID { get; set; }
        public Order? Order { get; set; }

        public int VersionNumber { get; set; }

        [Required, MaxLength(50)]
        public string ChangeType { get; set; }

        [Required, MaxLength(500)]
        public string ChangeDescription { get; set; }

        [Required]
        public string ChangedBy { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
    }
}
