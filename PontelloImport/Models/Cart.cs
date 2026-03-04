namespace PontelloImport.Models
{
    public class Cart
    {
        public int CartID { get; set; }

        public int DealerID { get; set; }
        public Dealer? Dealer { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
