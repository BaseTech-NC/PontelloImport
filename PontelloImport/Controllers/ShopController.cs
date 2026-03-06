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
                query = query.Where(p => p.Title.Contains(search));

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
        public async Task<IActionResult> AddToCart(int productVariantId, int quantity)
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
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                if (quantity <= 0)
                    _context.CartItems.Remove(item);
                else
                    item.Quantity = quantity;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Cart));
        }

        // POST: /Shop/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
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
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Load cart
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
                    Status = "Submitted"
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
