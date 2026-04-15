using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.ViewModels;

namespace PontelloImport.Controllers
{
    [Authorize]
    public class DashboardController : AdminBaseController
    {
        public DashboardController(PontelloDbContext context)
            : base(context)
        {
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
        public async Task<IActionResult> Admin()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var vm = new AdminDashboardViewModel
            {
                OrdersToday = await _context.Orders
                    .CountAsync(o => o.CreatedDate >= today),

                PendingReview = await _context.Orders
                    .CountAsync(o => o.Status == "Submitted"),

                ActionRequired = await _context.Orders
                    .CountAsync(o => o.Status == "ActionRequired"),

                RevenueThisMonth = await _context.Orders
                    .Where(o => o.CreatedDate >= monthStart
                             && o.Status != "Cancelled")
                    .SumAsync(o => (decimal?)o.SubtotalAmount) ?? 0,

                ActiveDealers = await _context.Dealers.CountAsync(),

                PendingApplications = await _context.DealerApplications
                    .CountAsync(a => a.Status == "Pending"),

                SubmittedCount = await _context.Orders.CountAsync(o => o.Status == "Submitted"),
                ConfirmedCount = await _context.Orders.CountAsync(o => o.Status == "Confirmed"),
                ShippedCount   = await _context.Orders.CountAsync(o => o.Status == "Shipped"),
                InvoicedThisMonth = await _context.Orders
                    .CountAsync(o => o.Status == "Invoiced" && o.CreatedDate >= monthStart),

                RecentOrders = await _context.Orders
                    .Include(o => o.Dealer)
                    .Where(o => o.Status == "Submitted" || o.Status == "ActionRequired")
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(8)
                    .ToListAsync(),

                RecentActivity = await _context.OrderHistories
                    .Include(h => h.Order)
                        .ThenInclude(o => o!.Dealer)
                    .OrderByDescending(h => h.ChangedDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }

    }
}
