namespace PontelloImport.Models
{
    public class DealerSummaryViewModel
    {
        public Dealer Dealer { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        public bool IsActive { get; set; }
        public int OrderCount { get; set; }
    }

    public class DealerProfileViewModel
    {
        public Dealer Dealer { get; set; } = null!;
        public ApplicationUser? User { get; set; }
        public bool IsActive { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public decimal TotalOrderValue { get; set; }
        public int TotalOrderCount { get; set; }
        public List<PaymentTerms> AllPaymentTerms { get; set; } = new();
    }
}
