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
    }
}
