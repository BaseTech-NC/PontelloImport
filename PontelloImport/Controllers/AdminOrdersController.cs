using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminOrdersController : Controller
    {
        private readonly PontelloDbContext _context;
        private readonly ILogger<AdminOrdersController> _logger;

        public AdminOrdersController(PontelloDbContext context, ILogger<AdminOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string CurrentUser => User.Identity?.Name ?? "Admin";

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

            _logger.LogInformation("Inventory deducted for order {OrderId}: {Count} lines", id, order.OrderLines.Count);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Order confirmed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/FlagIssue/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagIssue(int id, string? reason)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted")
            {
                TempData["Error"] = $"Cannot flag an issue on an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "A reason is required to flag an issue.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "ActionRequired";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "FlaggedIssue",
                ChangeDescription = reason.Trim(),
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Issue flagged. Dealer has been notified to contact Pontello.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? cancelNote)
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
                ChangeDescription = !string.IsNullOrWhiteSpace(cancelNote)
                    ? cancelNote.Trim()
                    : "Order cancelled by Pontello",
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

        // POST: /AdminOrders/ResolveAndConfirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveAndConfirm(int id, string? resolutionNote)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "ActionRequired")
            {
                TempData["Error"] = $"Cannot resolve an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Confirmed";

            foreach (var line in order.OrderLines)
            {
                if (!line.ProductVariantID.HasValue) continue;
                var variant = await _context.ProductVariants.FindAsync(line.ProductVariantID.Value);
                if (variant != null)
                    variant.InventoryQuantity = Math.Max(0, variant.InventoryQuantity - line.Quantity);
            }

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Resolved",
                ChangeDescription = !string.IsNullOrWhiteSpace(resolutionNote)
                    ? resolutionNote.Trim()
                    : "Issue resolved. Order confirmed by admin.",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order resolved and confirmed.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /AdminOrders/EditLines/5
        [HttpGet]
        public async Task<IActionResult> EditLines(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "ActionRequired")
            {
                TempData["Error"] = "Order lines can only be edited for ActionRequired orders.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(order);
        }

        // POST: /AdminOrders/EditLines/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLines(int id, int[] lineIds, int[] quantities, string? adminNote)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "ActionRequired")
            {
                TempData["Error"] = "Order lines can only be edited for ActionRequired orders.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var removedLineIds = new HashSet<int>();
            var modifiedCount = 0;

            for (int i = 0; i < lineIds.Length && i < quantities.Length; i++)
            {
                var line = order.OrderLines.FirstOrDefault(l => l.OrderLineID == lineIds[i]);
                if (line == null) continue;

                if (quantities[i] <= 0)
                {
                    _context.OrderLines.Remove(line);
                    removedLineIds.Add(lineIds[i]);
                    modifiedCount++;
                }
                else if (line.Quantity != quantities[i])
                {
                    line.Quantity = quantities[i];
                    line.LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2);
                    modifiedCount++;
                }
            }

            // Recalculate order totals from remaining lines
            var remaining = order.OrderLines
                .Where(l => !removedLineIds.Contains(l.OrderLineID))
                .ToList();
            order.SubtotalAmount = remaining.Sum(l => l.LineTotal);
            order.TaxAmount = order.IsTaxExempt ? null : Math.Round(order.SubtotalAmount * (order.TaxRate ?? 0.13m), 2);
            order.TotalAmount = order.SubtotalAmount + (order.TaxAmount ?? 0m) + (order.ShippingCost ?? 0m);

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "AdminModified",
                ChangeDescription = !string.IsNullOrWhiteSpace(adminNote)
                    ? adminNote.Trim()
                    : $"Order lines updated. {modifiedCount} line{(modifiedCount != 1 ? "s" : "")} modified.",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order lines updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/SaveTracking/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTracking(int id, string? TrackingNumber)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = $"Cannot save tracking on an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.TrackingNumber = TrackingNumber;
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "TrackingAdded",
                ChangeDescription = $"Tracking number added: {TrackingNumber}",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Tracking number saved. Click Mark as Shipped when ready to ship.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /AdminOrders/MarkShipped/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = $"Cannot mark shipped an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.Status = "Shipped";
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "Shipped",
                ChangeDescription = string.IsNullOrEmpty(order.TrackingNumber)
                    ? "Order marked as shipped"
                    : $"Shipped. Tracking: {order.TrackingNumber}",
                ChangedBy = CurrentUser
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Order marked as shipped.";
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
