using PontelloImport.Models;

namespace PontelloImport.ViewModels
{
    public class DealerDashboardViewModel
    {
        public Dealer Dealer { get; set; } = null!;
        public int TotalOrders { get; set; }
        public int ActiveOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Notification> UnreadNotifications { get; set; } = new();
    }
}
