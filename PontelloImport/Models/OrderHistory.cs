using System.ComponentModel.DataAnnotations;

namespace PontelloImport.Models
{
    public class OrderHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required(ErrorMessage = "Order ID is required")]
        [Display(Name = "Order ID")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Previous status is required")]
        [Display(Name = "Previous Status")]
        public OrderStatus PreviousStatus { get; set; }

        [Required(ErrorMessage = "New status is required")]
        [Display(Name = "New Status")]
        public OrderStatus NewStatus { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? ChangeDescription { get; set; }

        [Required(ErrorMessage = "Changed By is required")]
        [Display(Name = "Changed By")]
        public int ChangedBy { get; set; }

        [Required(ErrorMessage = "Change date is required")]
        [Display(Name = "Changed Date")]
        public DateTime ChangedDate { get; set; }
    }
}
