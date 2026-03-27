using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PontelloImport.Models
{
    public class OrderLine
    {
        public int OrderLineID { get; set; }

        public int OrderID { get; set; }
        public Order? Order { get; set; }

        public int? ProductVariantID { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        [Required, MaxLength(100)]
        public string SKU { get; set; }

        [Required, MaxLength(255)]
        public string ProductTitle { get; set; }

        [MaxLength(255)]
        public string? VariantTitle { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; set; }
    }
}
