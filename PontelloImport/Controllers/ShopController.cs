using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    public class ShopController : Controller
    {
        private readonly PontelloDbContext _context;
        private const int HardcodedDealerID = 1;

        public ShopController(PontelloDbContext context)
        {
            _context = context;
        }

        // GET: /Shop
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            search = search?.Trim();

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

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);

            if (cart == null)
            {
                cart = new Cart { DealerID = HardcodedDealerID };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantID == productVariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartID = cart.CartID,
                    ProductVariantID = productVariantId,
                    Quantity = quantity
                });
            }

            cart.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Item added to cart.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(Cart));
        }

        // GET: /Shop/Cart
        public async Task<IActionResult> Cart()
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);

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
                var cart = await _context.Carts
                    .Include(c => c.CartItems).ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);
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
                var cart = await _context.Carts
                    .Include(c => c.CartItems).ThenInclude(ci => ci.ProductVariant)
                    .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);
                var cartTotal = cart?.CartItems.Sum(ci => ci.Quantity * (ci.ProductVariant?.Price ?? 0m)) ?? 0m;
                return Json(new { success = true, cartTotal = cartTotal.ToString("F2") });
            }

            return RedirectToAction(nameof(Cart));
        }

        // GET: /Shop/Checkout
        public async Task<IActionResult> Checkout()
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(pv => pv!.Product)
                .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var dealer = await _context.Dealers
                .Include(d => d.BillingAddress)
                .Include(d => d.PaymentTerms)
                .FirstOrDefaultAsync(d => d.DealerID == HardcodedDealerID);

            ViewBag.Dealer = dealer;
            return View(cart);
        }

        // POST: /Shop/SubmitOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOrder()
        {
            // Guard: must have items before starting a transaction
            var cartCheck = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);
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
                    .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction(nameof(Cart));
                }

                // 2. Load dealer with PaymentTerms
                var dealer = await _context.Dealers
                    .Include(d => d.PaymentTerms)
                    .FirstOrDefaultAsync(d => d.DealerID == HardcodedDealerID);

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
                    DealerID = HardcodedDealerID,
                    DealerCompanyName = dealer.CompanyName,
                    SubtotalAmount = subtotal,
                    IsTaxExempt = dealer.IsTaxExempt,
                    TaxRate = taxRate,
                    TaxAmount = taxAmount,
                    TotalAmount = total,
                    PaymentTermsID = dealer.PaymentTermsID,
                    PaymentDueDate = DateTime.UtcNow.AddDays(dealer.PaymentTerms.DaysUntilDue),
                    Status = "Submitted",
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
                    ChangedBy = "dealer@testdealer.com"
                });

                // 12. Delete cart (cascade removes CartItems)
                _context.Carts.Remove(cart);

                // 13. Save and commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 14. Redirect to confirmation
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
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: /Shop/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderLines)
                .Where(o => o.DealerID == HardcodedDealerID)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: /Shop/Reorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int orderId)
        {
            var originalOrder = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.OrderID == orderId && o.DealerID == HardcodedDealerID);

            if (originalOrder == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.DealerID == HardcodedDealerID);

            if (cart == null)
            {
                cart = new Cart { DealerID = HardcodedDealerID };
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

            // Store source order ID so SubmitOrder can set PreviousOrderID
            HttpContext.Session.SetString("ReorderSourceID", orderId.ToString());

            if (skippedCount > 0)
                TempData["Warning"] = "Some items from this order are no longer available and were not added to your cart.";

            TempData["Success"] = "Items added to your cart. Review and submit when ready.";
            return RedirectToAction(nameof(Cart));
        }

        // GET: /Shop/OrderDetail/{id}
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.OrderHistories.OrderBy(h => h.ChangedDate))
                .Include(o => o.PaymentTerms)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.DealerID == HardcodedDealerID);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
