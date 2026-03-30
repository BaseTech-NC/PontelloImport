using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        [Required, MaxLength(10)]
        public string OrderNumber { get; set; }

        public int VersionNumber { get; set; } = 0;
        public bool IsCurrentVersion { get; set; } = true;

        public int? RootOrderID { get; set; }
        public Order? RootOrder { get; set; }

        public int DealerID { get; set; }
        public Dealer? Dealer { get; set; }

        [Required, MaxLength(255)]
        public string DealerCompanyName { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal SubtotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ShippingCost { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        public bool IsTaxExempt { get; set; } = false;

        [Column(TypeName = "decimal(5,4)")]
        public decimal? TaxRate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TaxAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        public int PaymentTermsID { get; set; }
        public PaymentTerms? PaymentTerms { get; set; }

        public DateTime? PaymentDueDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public int? PreviousOrderID { get; set; }
        public Order? PreviousOrder { get; set; }

        public bool DealerHasViewed { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string PONumber => VersionNumber == 0 ? OrderNumber : $"{OrderNumber}-{VersionNumber}";

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
        public ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();
    }
}
