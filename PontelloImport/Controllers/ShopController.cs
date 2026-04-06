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
    [Authorize(Roles = "Dealer")]
    public class ShopController : Controller
    {
        private readonly PontelloDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly ILogger<ShopController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopController(
            PontelloDbContext context,
            IEmailService emailService,
            IPdfService pdfService,
            ILogger<ShopController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _pdfService = pdfService;
            _logger = logger;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentDealerIdAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;
            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.ApplicationUserID == userId);
            return dealer?.DealerID;
        }

        private async Task SetDealerViewData(int dealerId)
        {
            ViewData["UnviewedOrderCount"] = await _context.Orders
                .CountAsync(o => o.DealerID == dealerId && !o.DealerHasViewed);

            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.DealerID == dealerId);
            if (dealer != null)
            {
                ViewData["DealerCompanyName"] = dealer.CompanyName;

                if (dealer.ApplicationUserID != null)
                {
                    var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);
                    if (user != null)
                        ViewData["DealerFirstName"] = user.FirstName;
                }
            }

            // Dealer notifications (newest 15)
            var dealerNotifications = await _context.Notifications
                .Where(n => n.DealerID == dealerId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(15)
                .ToListAsync();

            ViewData["DealerNotifications"] = dealerNotifications;
            ViewData["DealerUnreadCount"]   = dealerNotifications.Count(n => !n.IsRead);
        }

        private async Task SetUnviewedOrderCount(int dealerId)
        {
            await SetDealerViewData(dealerId);
        }

        private IActionResult DealerNotFound()
        {
            TempData["Error"] = "Dealer account not found. Please contact Pontello Imports.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Shop
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            search = search?.Trim();

            var dealerIdForCount = await GetCurrentDealerIdAsync();
            if (dealerIdForCount.HasValue)
                await SetUnviewedOrderCount(dealerIdForCount.Value);

            var categories = await _context.ProductCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            var query = _context.Products
                .Include(p => p.Vendor)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants.Where(v => v.Status == ProductStatus.Published))
                .Where(p => p.Status == ProductStatus.Published)
                .Where(p => p.ProductVariants.Any(v => v.Status == ProductStatus.Published))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.ToLower().Contains(search.ToLower()));

            if (categoryId.HasValue)
                query = query.Where(p => p.ProductCategoryID == categoryId.Value);

            var products = await query.OrderBy(p => p.Title).ToListAsync();

            return View(products);
        }

        // GET: /Shop/Product/{id}
        public async Task<IActionResult> Product(int id)
        {
            var product = await _context.Products
                .Include(p => p.Vendor)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductVariants.Where(v => v.Status == ProductStatus.Published))
                .Include(p => p.ProductSpecifications.OrderBy(s => s.DisplayOrder))
                .FirstOrDefaultAsync(p => p.ProductID == id && p.Status == ProductStatus.Published);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Shop/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productVariantId, int quantity, string? returnUrl)
        {
            if (quantity <= 0) quantity = 1;

            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);

            if (cart == null)
            {
                cart = new Cart { DealerID = dealerId.Value };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Check stock policy before adding
            var variant = await _context.ProductVariants.FindAsync(productVariantId);
            if (variant == null)
            {
                TempData["Error"] = "Product variant not found.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction(nameof(Cart));
            }
            if (variant.StockPolicy == "deny" && variant.InventoryQuantity <= 0)
            {
                TempData["Error"] = "This item is currently out of stock and cannot be ordered.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction(nameof(Cart));
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantID == productVariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(existingItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductVariantID = productVariantId,
                    Quantity = quantity
                });
            }

            cart.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var product = await _context.ProductVariants
                .Where(i => i.VariantID == productVariantId)
                .FirstOrDefaultAsync();

            if (product != null && existingItem.Quantity > product.InventoryQuantity)
            {
                TempData["Warning"] = $"Only {product.InventoryQuantity} units available for selected item variant — your order will proceed but Pontello will contact you.";
            }
            else
            {
                TempData["Success"] = "Item added to cart.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Cart));
        }

        // GET: /Shop/Cart
        public async Task<IActionResult> Cart()
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            await SetUnviewedOrderCount(dealerId.Value);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);

            return View(cart);
        }

        // POST: /Shop/UpdateCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var item = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.CartItemID == cartItemId);

            if (item == null)
            {
                if (isAjax) return Json(new { success = false, error = "Item not found" });
                return RedirectToAction(nameof(Cart));
            }

            decimal unitPrice = item.ProductVariant?.Price ?? 0m;
            if (quantity <= 0)
                _context.CartItems.Remove(item);
            else
                item.Quantity = quantity;

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                var dealerId2 = await GetCurrentDealerIdAsync();
                var cart = dealerId2.HasValue
                    ? await _context.Carts
                        .Include(c => c.CartItems).ThenInclude(ci => ci.ProductVariant)
                        .FirstOrDefaultAsync(c => c.DealerID == dealerId2.Value)
                    : null;
                var cartTotal = cart?.CartItems.Sum(ci => ci.Quantity * (ci.ProductVariant?.Price ?? 0m)) ?? 0m;
                var lineTotal = quantity <= 0 ? 0m : Math.Round(quantity * unitPrice, 2);
                return Json(new { success = true, lineTotal = lineTotal.ToString("F2"), cartTotal = cartTotal.ToString("F2"), removed = quantity <= 0 });
            }

            return RedirectToAction(nameof(Cart));
        }

        // POST: /Shop/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            if (isAjax)
            {
                var dealerId2 = await GetCurrentDealerIdAsync();
                var cart = dealerId2.HasValue
                    ? await _context.Carts
                        .Include(c => c.CartItems).ThenInclude(ci => ci.ProductVariant)
                        .FirstOrDefaultAsync(c => c.DealerID == dealerId2.Value)
                    : null;
                var cartTotal = cart?.CartItems.Sum(ci => ci.Quantity * (ci.ProductVariant?.Price ?? 0m)) ?? 0m;
                return Json(new { success = true, cartTotal = cartTotal.ToString("F2") });
            }

            return RedirectToAction(nameof(Cart));
        }

        // GET: /Shop/Checkout
        public async Task<IActionResult> Checkout()
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var dealer = await _context.Dealers
                .Include(d => d.BillingAddress)
                .Include(d => d.ShippingAddress)
                .Include(d => d.PaymentTerms)
                .FirstOrDefaultAsync(d => d.DealerID == dealerId.Value);

            ViewBag.Dealer = dealer;

            // All addresses for checkout address selection
            var allAddresses = await _context.Addresses
                .Where(a => a.DealerID == dealerId.Value)
                .ToListAsync();
            // Include legacy billing/shipping if not already in the list
            if (dealer?.BillingAddress != null &&
                allAddresses.All(a => a.AddressID != dealer.BillingAddressID))
                allAddresses.Insert(0, dealer.BillingAddress);
            if (dealer?.ShippingAddress != null &&
                allAddresses.All(a => a.AddressID != dealer.ShippingAddressID))
                allAddresses.Add(dealer.ShippingAddress);

            var billingAddresses = allAddresses
                .Where(a => a.AddressType == "Billing" || a.AddressType == "Both"
                         || a.AddressType == null)
                .ToList();
            var shippingAddresses = allAddresses
                .Where(a => a.AddressType == "Shipping" || a.AddressType == "Both"
                         || a.AddressType == null)
                .ToList();
            var defaultAddress = allAddresses.FirstOrDefault(a => a.IsDefault)
                                 ?? allAddresses.FirstOrDefault();

            ViewBag.DealerAddresses = allAddresses;
            ViewBag.BillingAddresses = billingAddresses;
            ViewBag.ShippingAddresses = shippingAddresses;
            ViewBag.DefaultAddressId = defaultAddress?.AddressID;

            var flaggedItems = cart.CartItems
                .Where(i => i.ProductVariant.InventoryQuantity < i.Quantity)
                .Select(i => new CartItem
                {
                    CartItemID = i.CartItemID,
                    Cart = i.Cart,
                    ProductVariantID = i.ProductVariantID,
                    ProductVariant = i.ProductVariant
                })
                .ToList();

            if (flaggedItems != null && flaggedItems.Count > 0)
            {
                string warning = "";
                foreach(CartItem item in flaggedItems)
                {
                    warning += $"Only {item.ProductVariant.InventoryQuantity} units available for {item.ProductVariant.Product.Title}.\n\n";
                }
                warning += "Your order will proceed but Pontello will contact you.";
                TempData["Warning"] = warning;
            }

            return View(cart);
        }

        // POST: /Shop/SubmitOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder()
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            // Guard: must have items before starting a transaction
            var cartCheck = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);
            if (cartCheck == null || !cartCheck.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Load cart (full include for order creation)
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.ProductVariant)
                            .ThenInclude(pv => pv!.Product)
                    .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction(nameof(Cart));
                }

                // 1b. Block items with deny policy and zero stock
                var blockedItems = cart.CartItems
                    .Where(ci => ci.ProductVariant!.StockPolicy == "deny" && ci.ProductVariant.InventoryQuantity <= 0)
                    .ToList();
                if (blockedItems.Any())
                {
                    var names = string.Join(", ", blockedItems.Select(ci => ci.ProductVariant!.Product!.Title));
                    TempData["Error"] = $"The following items are out of stock and cannot be ordered: {names}. Please remove them from your cart.";
                    return RedirectToAction(nameof(Cart));
                }

                // 2. Load dealer with PaymentTerms
                var dealer = await _context.Dealers
                    .Include(d => d.PaymentTerms)
                    .FirstOrDefaultAsync(d => d.DealerID == dealerId.Value);

                if (dealer == null || dealer.PaymentTerms == null)
                {
                    TempData["Error"] = "Dealer configuration error. Please contact support.";
                    return RedirectToAction(nameof(Cart));
                }

                // 3. Lock and increment OrderSequence
                var seq = await _context.OrderSequence.FirstAsync();
                seq.LastUsedNumber++;

                // 4. Generate order number
                var orderNumber = seq.LastUsedNumber.ToString("D4");

                // 5. Calculate subtotal
                var subtotal = cart.CartItems.Sum(ci =>
                    Math.Round(ci.Quantity * ci.ProductVariant!.Price, 2));

                // 6-8. Tax
                var taxRate = dealer.IsTaxExempt ? (decimal?)null : 0.13m;
                var taxAmount = dealer.IsTaxExempt
                    ? (decimal?)null
                    : Math.Round(subtotal * 0.13m, 2);
                var total = subtotal + (taxAmount ?? 0);

                // 9. Create order
                int? previousOrderId = null;
                var reorderSourceIdStr = HttpContext.Session.GetString("ReorderSourceID");
                if (int.TryParse(reorderSourceIdStr, out var parsedSourceId))
                {
                    previousOrderId = parsedSourceId;
                    HttpContext.Session.Remove("ReorderSourceID");
                }

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    DealerID = dealerId.Value,
                    DealerCompanyName = dealer.CompanyName,
                    SubtotalAmount = subtotal,
                    IsTaxExempt = dealer.IsTaxExempt,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    TotalAmount = total,
                    PaymentTermsID = dealer.PaymentTermsID,
                    PaymentDueDate = DateTime.UtcNow.AddDays(dealer.PaymentTerms.DaysUntilDue),
                    Status = "Submitted",
                    DealerHasViewed = true,
                    PreviousOrderID = previousOrderId
                };
                _context.Orders.Add(order);

                // 10. Create order lines (snapshots)
                foreach (var item in cart.CartItems)
                {
                    var v = item.ProductVariant!;
                    var optionValues = new[] { v.Option1Value, v.Option2Value, v.Option3Value, v.Option4Value, v.Option5Value }
                        .Where(o => !string.IsNullOrWhiteSpace(o));
                    var variantTitle = optionValues.Any() ? string.Join(" / ", optionValues) : null;

                    order.OrderLines.Add(new OrderLine
                    {
                        ProductVariantID = item.ProductVariantID,
                        SKU = v.SKU,
                        ProductTitle = v.Product!.Title,
                        VariantTitle = variantTitle,
                        Quantity = item.Quantity,
                        UnitPrice = v.Price,
                        LineTotal = Math.Round(item.Quantity * v.Price, 2)
                    });
                }

                // 11. Create order history entry
                order.OrderHistories.Add(new OrderHistory
                {
                    VersionNumber = 0,
                    ChangeType = "Submitted",
                    ChangeDescription = "Order submitted by dealer",
                    ChangedBy = User.Identity?.Name ?? "dealer"
                });

                // 12. Delete cart (cascade removes CartItems)
                _context.Carts.Remove(cart);

                // 13. Save and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 14. Admin notification for new order
                try
                {
                    _context.Notifications.Add(new PontelloImport.Models.Notification
                    {
                        DealerID  = null,
                        Type      = "OrderSubmitted",
                        Message   = $"New order #{order.OrderNumber} submitted by {dealer.CompanyName}",
                        ActionUrl = $"/AdminOrders/Details/{order.OrderID}",
                        IsRead    = false,
                        CreatedDate = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create order notification for {OrderNumber}", order.OrderNumber);
                }

                // 15. Generate PDF and email to both parties
                try
                {
                    // Reload order with full dealer + address includes for PDF
                    var orderForPdf = await _context.Orders
                        .Include(o => o.OrderLines)
                        .Include(o => o.PaymentTerms)
                        .Include(o => o.Dealer)
                            .ThenInclude(d => d!.BillingAddress)
                        .Include(o => o.Dealer)
                            .ThenInclude(d => d!.ShippingAddress)
                        .FirstOrDefaultAsync(o => o.OrderID == order.OrderID);

                    var dealerUser = await _userManager.FindByIdAsync(dealer.ApplicationUserID!);
                    var dealerEmail = dealerUser?.Email;

                    if (orderForPdf != null && dealerEmail != null)
                    {
                        var pdfBytes = _pdfService.GeneratePurchaseOrder(
                            orderForPdf, dealerEmail);
                        await _emailService.SendPurchaseOrderAsync(
                            dealerEmail,
                            dealer.CompanyName,
                            "noreply.pontelloimports@gmail.com",
                            order.OrderNumber,
                            pdfBytes);
                    }
                    else
                    {
                        // Fallback: plain admin notification
                        var summary = string.Join("\n",
                            order.OrderLines.Select(l =>
                                $"{l.ProductTitle} x{l.Quantity} — ${l.LineTotal:F2}"));
                        summary += $"\n\nTotal: ${order.TotalAmount:F2}";
                        await _emailService.SendOrderSubmittedAsync(
                            "noreply.pontelloimports@gmail.com",
                            dealer.CompanyName,
                            order.OrderNumber,
                            summary);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Email failed for order {Po}", order.OrderNumber);
                }

                // 15. Redirect to confirmation
                TempData["Success"] = $"Order {order.PONumber} submitted successfully.";
                return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderID });
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while submitting your order. Please try again.";
                return RedirectToAction(nameof(Checkout));
            }
        }

        // GET: /Shop/OrderConfirmation/{id}
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Dealer)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            if (order.Dealer != null)
            {
                var dealerUser = await _userManager.FindByIdAsync(order.Dealer.ApplicationUserID!);
                if (dealerUser != null)
                    ViewData["DealerEmail"] = dealerUser.Email;
            }

            return View(order);
        }

        // GET: /Shop/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            await SetUnviewedOrderCount(dealerId.Value);

            var orders = await _context.Orders
                .Include(o => o.OrderLines)
                .Where(o => o.DealerID == dealerId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: /Shop/Reorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int orderId)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var originalOrder = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.DealerID == dealerId.Value);

            if (originalOrder == null)
                return NotFound();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.DealerID == dealerId.Value);

                if (cart == null)
                {
                    cart = new Cart { DealerID = dealerId.Value };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var skippedCount = 0;
                foreach (var line in originalOrder.OrderLines)
                {
                    if (!line.ProductVariantID.HasValue)
                    {
                        skippedCount++;
                        continue;
                    }

                    var variant = await _context.ProductVariants.FindAsync(line.ProductVariantID.Value);
                    if (variant == null || variant.Status == ProductStatus.Archived)
                    {
                        skippedCount++;
                        continue;
                    }

                    var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantID == variant.VariantID);
                    if (existingItem != null)
                        existingItem.Quantity += line.Quantity;
                    else
                        cart.CartItems.Add(new CartItem
                        {
                            CartID = cart.CartID,
                            ProductVariantID = variant.VariantID,
                            Quantity = line.Quantity
                        });
                }

                cart.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Store source order ID so SubmitOrder can set PreviousOrderID
                HttpContext.Session.SetString("ReorderSourceID", orderId.ToString());

                if (skippedCount > 0)
                    TempData["Warning"] = "Some items from this order are no longer available and were not added to your cart.";

                TempData["Success"] = "Items added to your cart. Review and submit when ready.";
                return RedirectToAction(nameof(Cart));
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while loading your cart. Please try again.";
                return RedirectToAction(nameof(OrderHistory));
            }
        }

        // GET: /Shop/OrderDetail/{id}
        public async Task<IActionResult> OrderDetail(int id)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.OrderHistories.OrderBy(h => h.ChangedDate))
                .Include(o => o.PaymentTerms)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.DealerID == dealerId.Value);

            if (order == null)
                return NotFound();

            if (order.Status == "ActionRequired")
            {
                var flaggedIssue = order.OrderHistories
                    .Where(h => h.ChangeType == "FlaggedIssue")
                    .OrderByDescending(h => h.ChangedDate)
                    .FirstOrDefault();
                ViewData["FlaggedIssueHistory"] = flaggedIssue;
            }

            await SetUnviewedOrderCount(dealerId.Value);

            if (!order.DealerHasViewed)
            {
                order.DealerHasViewed = true;
                await _context.SaveChangesAsync();
            }

            return View(order);
        }

        // POST: /Shop/DealerMarkNotificationRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealerMarkNotificationRead(int id)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == dealerId.Value)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OrderHistory));
        }

        // POST: /Shop/DealerMarkAllNotificationsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealerMarkAllNotificationsRead()
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var unread = await _context.Notifications
                .Where(n => n.DealerID == dealerId.Value && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread)
                n.IsRead = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(OrderHistory));
        }

        // POST: /Shop/DealerDismissNotification/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealerDismissNotification(int id)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return DealerNotFound();

            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == dealerId.Value)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(OrderHistory));
        }

        // POST: /Shop/DealerMarkNotificationReadJson/5  (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealerMarkNotificationReadJson(int id)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return Json(new { ok = false });

            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == dealerId.Value)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Json(new { ok = true });
        }

        // POST: /Shop/DealerDismissNotificationJson/5  (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DealerDismissNotificationJson(int id)
        {
            var dealerId = await GetCurrentDealerIdAsync();
            if (dealerId == null) return Json(new { ok = false });

            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == dealerId.Value)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
            return Json(new { ok = true });
        }
    }
}
