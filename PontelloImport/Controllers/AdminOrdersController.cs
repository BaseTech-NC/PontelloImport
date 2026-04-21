using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using PontelloImport.Services;
using System.Security.Claims;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class AdminOrdersController : AdminBaseController
    {
        private readonly ILogger<AdminOrdersController> _logger;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminOrdersController(
            PontelloDbContext context,
            ILogger<AdminOrdersController> logger,
            IEmailService emailService,
            IPdfService pdfService,
            UserManager<ApplicationUser> userManager)
            : base(context)
        {
            _logger = logger;
            _emailService = emailService;
            _pdfService = pdfService;
            _userManager = userManager;
        }

        private string CurrentUser => User.Identity?.Name ?? "Admin";

        private async Task<(string? email, string companyName)> GetDealerEmailAsync(Order order)
        {
            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.DealerID == order.DealerID);
            if (dealer?.ApplicationUserID == null)
                return (null, order.DealerCompanyName);
            var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);
            return (user?.Email, dealer.CompanyName);
        }

        private void QueueDealerNotification(int dealerId, string type, string message)
        {
            try
            {
                _context.Notifications.Add(new Notification
                {
                    DealerID    = dealerId,
                    Type        = type,
                    Message     = message,
                    ActionUrl   = "/Shop/OrderHistory",
                    IsRead      = false,
                    CreatedDate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue dealer notification");
            }
        }

        public async Task<IActionResult> Index(
            string? tab = "all",
            string? search = null,
            string? fulfillment = null,
            string? billing = null,
            int page = 1,
            int pageSize = 25,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            search = search?.Trim();

            // Tab counts — always against full dataset
            ViewData["AllCount"]       = await _context.Orders.CountAsync();
            ViewData["SubmittedCount"] = await _context.Orders.CountAsync(o => o.Status == "Submitted");
            ViewData["ActionCount"]    = await _context.Orders.CountAsync(o => o.Status == "ActionRequired");
            ViewData["ConfirmedCount"] = await _context.Orders.CountAsync(o => o.Status == "Confirmed");
            ViewData["CancelledCount"] = await _context.Orders.CountAsync(o => o.Status == "Cancelled");

            var query = _context.Orders
                .Include(o => o.Dealer)
                .Include(o => o.OrderLines)
                .AsQueryable();

            // Primary tab — workflow status only
            query = tab switch
            {
                "submitted"      => query.Where(o => o.Status == "Submitted"),
                "actionrequired" => query.Where(o => o.Status == "ActionRequired"),
                "confirmed"      => query.Where(o => o.Status == "Confirmed"),
                "cancelled"      => query.Where(o => o.Status == "Cancelled"),
                _                => query
            };

            // Secondary fulfillment filter
            query = fulfillment switch
            {
                "notshipped" => query.Where(o => o.FulfillmentStatus == "NotShipped"),
                "shipped"    => query.Where(o => o.FulfillmentStatus == "Shipped"),
                _            => query
            };

            // Secondary billing filter
            query = billing switch
            {
                "notinvoiced"         => query.Where(o => o.BillingStatus == "NotInvoiced"),
                "invoiced"            => query.Where(o => o.BillingStatus == "Invoiced"),
                "shipped_notinvoiced" => query.Where(o => o.FulfillmentStatus == "Shipped" && o.BillingStatus == "NotInvoiced"),
                "invoiced_notshipped" => query.Where(o => o.BillingStatus == "Invoiced" && o.FulfillmentStatus == "NotShipped"),
                _                     => query
            };

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

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["ActiveTab"]  = tab ?? "all";
            ViewData["Fulfillment"] = fulfillment;
            ViewData["Billing"]    = billing;
            ViewData["Search"]     = search;
            ViewData["Page"]       = page;
            ViewData["PageSize"]   = pageSize;
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewData["DateFrom"]   = dateFrom?.ToString("yyyy-MM-dd");
            ViewData["DateTo"]     = dateTo?.ToString("yyyy-MM-dd");

            return View(orders);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(int id)
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
            order.DealerHasViewed = false;

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
                ChangeType = "Confirmed",
                ChangeDescription = "Order confirmed by Pontello",
                ChangedBy = CurrentUser
            });

            QueueDealerNotification(order.DealerID,
                "OrderConfirmed",
                $"Your order #{order.OrderNumber} has been confirmed by Pontello Imports.");

            await _context.SaveChangesAsync();

            var (email, companyName) = await GetDealerEmailAsync(order);
            if (email != null)
            {
                try
                {
                    await _emailService.SendOrderStatusChangedAsync(
                        email, companyName, order.OrderNumber, "Confirmed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for order {Po}", order.OrderNumber);
                }
            }

            TempData["Success"] = "Order confirmed.";
            return RedirectToAction(nameof(Details), new { id });
        }

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
            order.DealerHasViewed = false;
            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID = id,
                VersionNumber = order.VersionNumber,
                ChangeType = "FlaggedIssue",
                ChangeDescription = reason.Trim(),
                ChangedBy = CurrentUser
            });

            QueueDealerNotification(order.DealerID,
                "IssueFlagged",
                $"Action required — PO #{order.OrderNumber}: {reason.Trim()}. Please contact Pontello Imports at 647-964-6833.");

            await _context.SaveChangesAsync();

            var (flagEmail, flagCompany) = await GetDealerEmailAsync(order);
            if (flagEmail != null)
            {
                try
                {
                    await _emailService.SendAsync(flagEmail, flagCompany,
                        $"Action required on Order #{order.OrderNumber}",
                        $"<div style='font-family:sans-serif;max-width:560px;'>" +
                        $"<p>There is an issue with your order <strong>PO #{order.OrderNumber}</strong>.</p>" +
                        $"<p><strong>Issue:</strong> {reason.Trim()}</p>" +
                        $"<p>Please contact Pontello Imports:<br>" +
                        $"Phone: 647-964-6833<br>" +
                        $"Email: jesse@pontelloimports.com</p>" +
                        $"</div>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FlagIssue email failed for order {Po}", order.OrderNumber);
                }
            }

            TempData["Success"] = "Issue flagged. Dealer has been notified to contact Pontello.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id, string? cancelNote)
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
            order.DealerHasViewed = false;

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

            var (email, companyName) = await GetDealerEmailAsync(order);
            if (email != null)
            {
                try
                {
                    await _emailService.SendOrderStatusChangedAsync(
                        email, companyName, order.OrderNumber, "Cancelled", cancelNote);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for order {Po}", order.OrderNumber);
                }
            }

            TempData["Success"] = "Order cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkShipped(
            int id,
            DateTime? shipDate,
            string? trackingNumber,
            string? carrier,
            decimal? shippingAmount)
        {
            if (!shipDate.HasValue)
            {
                TempData["Error"] = "Ship date is required to mark as shipped.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.FulfillmentStatus = "Shipped";
            order.ShipDate          = shipDate;
            order.TrackingNumber    = trackingNumber;
            order.Carrier           = carrier;

            if (shippingAmount.HasValue)
            {
                order.ShippingCost  = shippingAmount;
                order.TotalAmount   = order.SubtotalAmount + (order.TaxAmount ?? 0) + shippingAmount.Value;
            }

            var desc = $"Marked as shipped on {shipDate.Value:MMMM d, yyyy}." +
                (!string.IsNullOrEmpty(trackingNumber) ? $" Tracking: {trackingNumber}" : "") +
                (!string.IsNullOrEmpty(carrier) ? $" Carrier: {carrier}" : "") +
                (shippingAmount.HasValue ? $" Shipping: ${shippingAmount:F2}" : "");

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID           = id,
                VersionNumber     = order.VersionNumber,
                ChangeType        = "Shipped",
                ChangeDescription = desc,
                ChangedBy         = CurrentUser
            });

            QueueDealerNotification(order.DealerID,
                "OrderShipped",
                $"PO #{order.OrderNumber} was shipped on {shipDate.Value:MMMM d, yyyy}." +
                (!string.IsNullOrEmpty(trackingNumber) ? $" Tracking: {trackingNumber}" : ""));

            await _context.SaveChangesAsync();

            var (email, companyName) = await GetDealerEmailAsync(order);
            if (email != null)
            {
                try
                {
                    await _emailService.SendOrderStatusChangedAsync(
                        email, companyName, order.OrderNumber, "Shipped",
                        !string.IsNullOrEmpty(trackingNumber) ? $"Tracking number: {trackingNumber}" : null);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for order {Po}", order.OrderNumber);
                }
            }

            TempData["Success"] = "Order marked as shipped.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInvoiced(int id, string? invoiceNumber, string? invoiceNotes)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status != "Confirmed")
            {
                TempData["Error"] = $"Cannot invoice an order with status '{order.Status}'.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.BillingStatus  = "Invoiced";
            order.InvoiceNumber  = !string.IsNullOrWhiteSpace(invoiceNumber)
                ? invoiceNumber
                : $"INV-{order.OrderNumber}";
            order.InvoicedAt     = DateTime.UtcNow;
            order.InvoiceNotes   = invoiceNotes;
            order.DealerHasViewed = false;

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID           = id,
                VersionNumber     = order.VersionNumber,
                ChangeType        = "Invoiced",
                ChangeDescription = $"Invoice {order.InvoiceNumber} issued on {DateTime.Now:MMMM d, yyyy}.",
                ChangedBy         = CurrentUser
            });

            QueueDealerNotification(order.DealerID,
                "OrderInvoiced",
                $"PO #{order.OrderNumber} has been invoiced. Invoice: {order.InvoiceNumber}. Total: ${order.TotalAmount:F2}.");

            await _context.SaveChangesAsync();

            var (email, companyName) = await GetDealerEmailAsync(order);
            if (email != null)
            {
                try
                {
                    await _emailService.SendOrderStatusChangedAsync(
                        email, companyName, order.OrderNumber, "Invoiced");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for order {Po}", order.OrderNumber);
                }
            }

            TempData["Success"] = "Order marked as invoiced.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveShipping(
            int id,
            DateTime? shipDate,
            decimal? shippingAmount,
            string? trackingNumber,
            string? carrier)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (shipDate.HasValue)
                order.ShipDate   = shipDate;
            order.ShippingCost   = shippingAmount;
            order.TrackingNumber = trackingNumber;
            order.Carrier        = carrier;

            if (shippingAmount.HasValue)
                order.TotalAmount = order.SubtotalAmount + (order.TaxAmount ?? 0) + shippingAmount.Value;

            bool isInvoiced = order.BillingStatus == "Invoiced";
            if (isInvoiced)
                order.VersionNumber = order.VersionNumber + 1;

            var desc = "Shipping details updated." +
                (shipDate.HasValue ? $" Ship date: {shipDate.Value:MMMM d, yyyy}." : "") +
                (shippingAmount.HasValue ? $" Amount: ${shippingAmount:F2}." : "") +
                (!string.IsNullOrEmpty(trackingNumber) ? $" Tracking: {trackingNumber}." : "") +
                (!string.IsNullOrEmpty(carrier) ? $" Carrier: {carrier}." : "");

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID           = id,
                VersionNumber     = order.VersionNumber,
                ChangeType        = "ShippingUpdated",
                ChangeDescription = desc,
                ChangedBy         = CurrentUser
            });

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            if (isInvoiced)
            {
                try
                {
                    var fullOrder = await _context.Orders
                        .Include(o => o.OrderLines)
                        .Include(o => o.Dealer)
                            .ThenInclude(d => d.BillingAddress)
                        .Include(o => o.Dealer)
                            .ThenInclude(d => d.ShippingAddress)
                        .Include(o => o.PaymentTerms)
                        .FirstOrDefaultAsync(o => o.OrderID == id);

                    var revisedPo = $"{order.OrderNumber}-{order.VersionNumber}";
                    var (email, companyName) = await GetDealerEmailAsync(fullOrder!);

                    if (email != null && fullOrder != null)
                    {
                        var pdfBytes = _pdfService.GeneratePurchaseOrder(fullOrder, email);
                        await _emailService.SendPurchaseOrderAsync(
                            email,
                            companyName,
                            "noreply.pontelloimports@gmail.com",
                            revisedPo,
                            pdfBytes,
                            isRevised: true);
                        TempData["Success"] = $"Shipping saved. Revised PO {revisedPo} sent to dealer.";
                    }
                    else
                    {
                        TempData["Success"] = "Shipping saved.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Revised PO email failed for order {Id}", id);
                    TempData["Success"] = "Shipping saved.";
                }
            }
            else
            {
                TempData["Success"] = "Shipping saved.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInvoiceNotes(int id, string? invoiceNotes)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.InvoiceNotes = invoiceNotes;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Notes saved.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTax(int id, decimal taxRate)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.TaxRate     = taxRate / 100m;
            order.TaxAmount   = order.SubtotalAmount * (taxRate / 100m);
            order.TotalAmount = order.SubtotalAmount + order.TaxAmount.Value + (order.ShippingCost ?? 0);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> EditLines(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted" && order.Status != "ActionRequired")
            {
                TempData["Error"] = "Only Submitted or Action Required orders can be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Status == "Submitted")
            {
                order.Status = "ActionRequired";
                order.DealerHasViewed = false;
                _context.OrderHistories.Add(new OrderHistory
                {
                    OrderID           = id,
                    VersionNumber     = order.VersionNumber,
                    ChangeType        = "EditInitiated",
                    ChangeDescription = "Order opened for editing by admin.",
                    ChangedBy         = CurrentUser
                });
                await _context.SaveChangesAsync();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLines(int id, int[] lineIds, int[] quantities,
            string? editReason = null, string? editNote = null)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            if (order.Status != "Submitted" && order.Status != "ActionRequired")
            {
                TempData["Error"] = "Only Submitted or Action Required orders can be edited.";
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
                    line.Quantity  = quantities[i];
                    line.LineTotal = Math.Round(line.Quantity * line.UnitPrice, 2);
                    modifiedCount++;
                }
            }

            var newVariantIdStrings = Request.Form["newVariantIds[]"];
            var newQtyStrings       = Request.Form["newQuantities[]"];
            decimal newLinesSubtotal = 0m;

            for (int i = 0; i < newVariantIdStrings.Count; i++)
            {
                if (!int.TryParse(newVariantIdStrings[i], out int variantId)) continue;
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.VariantID == variantId);
                if (variant == null) continue;

                int qty = 1;
                if (i < newQtyStrings.Count && int.TryParse(newQtyStrings[i], out int parsedQty))
                    qty = Math.Max(1, parsedQty);

                var lineTotal = Math.Round(variant.Price * qty, 2);
                var opts = new[] { variant.Option1Value, variant.Option2Value, variant.Option3Value,
                                   variant.Option4Value, variant.Option5Value }
                    .Where(o => !string.IsNullOrWhiteSpace(o));
                var variantTitle = opts.Any() ? string.Join(" / ", opts) : null;

                _context.OrderLines.Add(new OrderLine
                {
                    OrderID          = id,
                    ProductVariantID = variant.VariantID,
                    ProductTitle     = variant.Product?.Title ?? "",
                    VariantTitle     = variantTitle,
                    SKU              = variant.SKU,
                    Quantity         = qty,
                    UnitPrice        = variant.Price,
                    LineTotal        = lineTotal
                });
                newLinesSubtotal += lineTotal;
                modifiedCount++;
            }

            var remaining = order.OrderLines
                .Where(l => !removedLineIds.Contains(l.OrderLineID))
                .ToList();

            order.SubtotalAmount = remaining.Sum(l => l.LineTotal) + newLinesSubtotal;
            order.TaxAmount      = order.IsTaxExempt ? null
                : Math.Round(order.SubtotalAmount * (order.TaxRate ?? 0.13m), 2);
            order.TotalAmount    = order.SubtotalAmount + (order.TaxAmount ?? 0m) + (order.ShippingCost ?? 0m);

            var fullNote = editReason != null
                ? (editNote != null ? $"{editReason}: {editNote}" : editReason)
                : editNote ?? $"Order lines updated. {modifiedCount} line{(modifiedCount != 1 ? "s" : "")} modified.";

            order.VersionNumber = order.VersionNumber + 1;
            var revisedPo = $"{order.OrderNumber}-{order.VersionNumber}";

            _context.OrderHistories.Add(new OrderHistory
            {
                OrderID           = id,
                VersionNumber     = order.VersionNumber,
                ChangeType        = "AdminModified",
                ChangeDescription = fullNote,
                ChangedBy         = CurrentUser
            });

            await _context.SaveChangesAsync();

            QueueDealerNotification(order.DealerID,
                "OrderModified",
                $"Pontello Imports updated your order PO #{revisedPo}. {fullNote}");
            await _context.SaveChangesAsync();

            try
            {
                var fullOrder = await _context.Orders
                    .Include(o => o.OrderLines)
                    .Include(o => o.Dealer)
                        .ThenInclude(d => d!.BillingAddress)
                    .Include(o => o.Dealer)
                        .ThenInclude(d => d!.ShippingAddress)
                    .Include(o => o.PaymentTerms)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (fullOrder != null)
                {
                    var (dealerEmail, companyName) = await GetDealerEmailAsync(fullOrder);
                    if (dealerEmail != null)
                    {
                        var pdfBytes = _pdfService.GeneratePurchaseOrder(fullOrder, dealerEmail);
                        await _emailService.SendPurchaseOrderAsync(
                            dealerEmail,
                            companyName,
                            "noreply.pontelloimports@gmail.com",
                            revisedPo,
                            pdfBytes,
                            isRevised: true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Revised PO email failed for order {Po}", order.OrderNumber);
            }

            TempData["Success"] = "Order lines updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(Array.Empty<object>());

            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Product.Status == ProductStatus.Published
                    && (v.Product.Title.Contains(q) || v.SKU.Contains(q)))
                .Take(6)
                .ToListAsync();

            var results = variants.Select(v =>
            {
                var opts = new[] { v.Option1Value, v.Option2Value, v.Option3Value,
                                   v.Option4Value, v.Option5Value }
                    .Where(o => !string.IsNullOrWhiteSpace(o));
                return new {
                    v.VariantID,
                    v.SKU,
                    v.Price,
                    v.InventoryQuantity,
                    ProductTitle = v.Product!.Title,
                    VariantTitle = opts.Any() ? string.Join(" / ", opts) : (string?)null
                };
            }).ToList();

            return Json(results);
        }

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
            sb.AppendLine("Order #,Date,Dealer,Status,FulfillmentStatus,BillingStatus,Subtotal,Tax,Shipping,Total,Items,Payment Terms");

            foreach (var order in orders)
            {
                sb.AppendLine(string.Join(",",
                    CsvField(order.PONumber),
                    order.OrderDate.ToString("yyyy-MM-dd"),
                    CsvField(order.DealerCompanyName),
                    CsvField(order.Status),
                    CsvField(order.FulfillmentStatus),
                    CsvField(order.BillingStatus),
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
    }
}
