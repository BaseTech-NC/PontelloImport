using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Order Number")]
        [StringLength(10)]
        public string OrderNumber { get; set; }

        [Display(Name = "Version Number")]
        public int VersionNumber { get; set; }

        [Display(Name = "Current Version")]
        public bool IsCurrentVersion { get; set; }

        [Required]
        [Display(Name = "Dealer ID")]
        public int DealerID { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Please input a valid subtotal amount.")]
        [Display(Name = "Subtotal Amount")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal SubtotalAmount { get; set; }

        [Required(ErrorMessage = "Please input a valid amount.")]
        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Please input a valid amount.")]
        [Display(Name = "Shipping Cost")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal? ShippingCost { get; set; }

        public int? ShippingCalculatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ShippingCalculatedDate { get; set; }

        [StringLength(1000, ErrorMessage = "Shipping notes cannot exceed 1000 characters.")]
        public string ShippingNotes { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        [Range(0, 1)]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 99999999.99)]
        public decimal? TaxAmount { get; set; }

        [Required]
        public int PaymentTermsID { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDueDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        public bool IsLocked { get; set; }

        public int? PreviousOrderId { get; set; }

        public string ModificationReason { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        public int? ModifiedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
    }
}
