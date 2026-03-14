using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly PontelloDbContext _context;

        public AdminOrdersController(PontelloDbContext context)
        {
            _context = context;
        }

        private string CurrentUser =>
            User.Identity?.Name
            ?? HttpContext.Session.GetString("DemoRole")
            ?? "Admin";

        // GET: /AdminOrders
        public async Task<IActionResult> Index(string? status, string? search)
        {
            var baseQuery = _context.Orders
                .Include(o => o.Dealer)
                .AsQueryable();

            // Status counts (before applying status filter so tabs always show full counts)
            ViewBag.AllCount            = await baseQuery.CountAsync();
            ViewBag.SubmittedCount      = await baseQuery.CountAsync(o => o.Status == "Submitted");
            ViewBag.ActionRequiredCount = await baseQuery.CountAsync(o => o.Status == "ActionRequired");
            ViewBag.ConfirmedCount      = await baseQuery.CountAsync(o => o.Status == "Confirmed");
            ViewBag.ShippedCount        = await baseQuery.CountAsync(o => o.Status == "Shipped");
            ViewBag.InvoicedCount       = await baseQuery.CountAsync(o => o.Status == "Invoiced");
            ViewBag.CancelledCount      = await baseQuery.CountAsync(o => o.Status == "Cancelled");

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
                baseQuery = baseQuery.Where(o => o.Status == status);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                baseQuery = baseQuery.Where(o =>
                    o.OrderNumber.ToLower().Contains(q) ||
                    o.DealerCompanyName.ToLower().Contains(q));
            }

            var orders = await baseQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;

            return View(orders);
        }

        // GET: /AdminOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.OrderHistories.OrderBy(h => h.ChangedDate))
                .Include(o => o.Dealer)
                .Include(o => o.PaymentTerms)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: /AdminOrders/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted" && order.Status != "ActionRequired")
            {
                TempData["Error"] = $"Cannot confirm an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Confirmed";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Confirmed",
                ChangeDescription = "Order confirmed by Pontello",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order confirmed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/FlagIssue/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagIssue(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted")
            {
                TempData["Error"] = $"Cannot flag an issue on an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "ActionRequired";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "ActionRequired",
                ChangeDescription = "Issue flagged — dealer notified",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Issue flagged.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted" && order.Status != "ActionRequired")
            {
                TempData["Error"] = $"Cannot cancel an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Cancelled";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Cancelled",
                ChangeDescription = "Order cancelled by Pontello",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/AddShipping/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddShipping(int id, decimal ShippingCost, string TrackingNumber)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = $"Cannot add shipping to an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.ShippingCost = ShippingCost;
            order.TrackingNumber = TrackingNumber;
            order.TotalAmount = order.SubtotalAmount + ShippingCost + (order.TaxAmount ?? 0m);
            order.Status = "Shipped";

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Shipped",
                ChangeDescription = $"Tracking: {TrackingNumber}",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Shipment recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/MarkInvoiced/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInvoiced(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Shipped")
            {
                TempData["Error"] = $"Cannot invoice an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Invoiced";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Invoiced",
                ChangeDescription = "Order invoiced",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order marked as invoiced.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
