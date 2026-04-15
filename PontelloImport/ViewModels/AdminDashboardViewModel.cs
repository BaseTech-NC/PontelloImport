using PontelloImport.Models;

namespace PontelloImport.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int OrdersToday { get; set; }
        public int PendingReview { get; set; }
        public int ActionRequired { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int ActiveDealers { get; set; }
        public int PendingApplications { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<OrderHistory> RecentActivity { get; set; } = new();

        // Order breakdown counts
        public int SubmittedCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int ShippedCount { get; set; }
        public int InvoicedThisMonth { get; set; }
    }
}
