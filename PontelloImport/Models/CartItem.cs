namespace PontelloImport.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }

        public int CartID { get; set; }
        public Cart? Cart { get; set; }

        public int ProductVariantID { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public int Quantity { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
