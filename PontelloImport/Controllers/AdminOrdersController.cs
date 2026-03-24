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
        public async Task<IActionResult> Index(
            string? status, string? search,
            int page = 1, int pageSize = 25,
            DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            search = search?.Trim();

            var baseQuery = _context.Orders
                .Include(o => o.Dealer)
                .AsQueryable();

            // Status counts (before any filters so tabs always show full counts)
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

            // Apply date filters
            if (dateFrom.HasValue)
                baseQuery = baseQuery.Where(o => o.OrderDate >= dateFrom.Value);
            if (dateTo.HasValue)
                baseQuery = baseQuery.Where(o => o.OrderDate <= dateTo.Value.AddDays(1));

            // Pagination
            var totalCount = await baseQuery.CountAsync();
            var orders = await baseQuery
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["DateFrom"] = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"] = dateTo?.ToString("yyyy-MM-dd");

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
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
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

            // Deduct inventory once on Confirm
            foreach (var line in order.OrderLines)
            {
                if (!line.ProductVariantID.HasValue) continue;
                var variant = await _context.ProductVariants.FindAsync(line.ProductVariantID.Value);
                if (variant != null)
                    variant.InventoryQuantity = Math.Max(0, variant.InventoryQuantity - line.Quantity);
            }

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
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted" && order.Status != "ActionRequired" && order.Status != "Confirmed")
            {
                TempData["Error"] = $"Cannot cancel an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var wasConfirmed = order.Status == "Confirmed";
            order.Status = "Cancelled";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Cancelled",
                ChangeDescription = "Order cancelled by Pontello",
                ChangedBy = CurrentUser
            });

            // Restore inventory only if cancelling from Confirmed (inventory was deducted at that point)
            if (wasConfirmed)
            {
                foreach (var line in order.OrderLines)
                {
                    if (!line.ProductVariantID.HasValue) continue;
                    var variant = await _context.ProductVariants.FindAsync(line.ProductVariantID.Value);
                    if (variant != null)
                        variant.InventoryQuantity += line.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/AddShipping/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddShipping(int id, decimal ShippingCost, string? TrackingNumber)
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
                ChangeDescription = string.IsNullOrWhiteSpace(TrackingNumber) ? "Shipping added" : $"Tracking: {TrackingNumber}",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Shipment recorded.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /AdminOrders/EditShipping/5
        [HttpGet]
        public async Task<IActionResult> EditShipping(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed" && order.Status != "Shipped")
            {
                TempData["Error"] = "Shipping can only be edited for Confirmed or Shipped orders.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(order);
        }

        // POST: /AdminOrders/EditShipping/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditShipping(int id, decimal shippingAmount)
        {
            if (shippingAmount < 0)
            {
                TempData["Error"] = "Shipping amount cannot be negative.";
                return RedirectToAction(nameof(EditShipping), new { id });
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.ShippingCost = shippingAmount;
            order.TotalAmount = order.SubtotalAmount + (order.TaxAmount ?? 0) + shippingAmount;

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "ShippingUpdated",
                ChangeDescription = $"Shipping updated to ${shippingAmount:F2}",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Shipping amount updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /AdminOrders/ExportCsv
        [HttpGet]
        public async Task<IActionResult> ExportCsv(
            string? statusFilter = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.PaymentTerms)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(o => o.Status == statusFilter);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(q) ||
                    o.DealerCompanyName.ToLower().Contains(q));
            }

            if (dateFrom.HasValue)
                query = query.Where(o => o.OrderDate >= dateFrom.Value);
            if (dateTo.HasValue)
                query = query.Where(o => o.OrderDate <= dateTo.Value.AddDays(1));

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            static string CsvField(string? s)
            {
                if (s == null) return "";
                if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
                    return "\"" + s.Replace("\"", "\"\"") + "\"";
                return s;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Order #,Date,Dealer,Company,Status,Subtotal,Tax,Shipping,Total,Items,Payment Terms");

            foreach (var order in orders)
            {
                sb.AppendLine(string.Join(",",
                    CsvField(order.PONumber),
                    order.OrderDate.ToString("yyyy-MM-dd"),
                    CsvField(order.DealerCompanyName),
                    CsvField(order.DealerCompanyName),
                    CsvField(order.Status),
                    order.SubtotalAmount.ToString("0.00"),
                    (order.TaxAmount ?? 0m).ToString("0.00"),
                    (order.ShippingCost ?? 0m).ToString("0.00"),
                    order.TotalAmount.ToString("0.00"),
                    order.OrderLines.Count.ToString(),
                    CsvField(order.PaymentTerms?.TermName ?? "")));
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"orders-{DateTime.Now:yyyy-MM-dd}.csv");
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
