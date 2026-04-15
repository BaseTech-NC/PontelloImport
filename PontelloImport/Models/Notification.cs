using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    /// <summary>
    /// Represents a notification for an admin user (DealerID = null)
    /// or a dealer user (DealerID = set).
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        /// <summary>
        /// null  → admin notification (visible to all admin/staff)
        /// set   → dealer notification (visible to that dealer only)
        /// </summary>
        public int? DealerID { get; set; }

        /// <summary>
        /// e.g. "OrderSubmitted", "ApplicationSubmitted",
        ///      "OrderConfirmed", "OrderShipped", "OrderInvoiced", "IssueFlagged"
        /// </summary>
        [Required, MaxLength(64)]
        public string Type { get; set; } = "";

        [Required, MaxLength(512)]
        public string Message { get; set; } = "";

        /// <summary>Optional URL the notification links to.</summary>
        [MaxLength(256)]
        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ── Navigation ───────────────────────────────────────────────────────
        [ForeignKey(nameof(DealerID))]
        public virtual Dealer? Dealer { get; set; }
    }
}
