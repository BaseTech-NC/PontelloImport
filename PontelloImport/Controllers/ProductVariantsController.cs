using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    public class ProductVariantsController : Controller
    {
        private readonly PontelloDbContext _context;
        private const string ReviewTempDataKey = "CreateProductReviewData";

        public ProductVariantsController(PontelloDbContext context)
        {
            _context = context;
        }

        // GET: ProductVariants
        public async Task<IActionResult> Index(
            string filter = "all",
            int pageNumber = 1,
            int pageSize = 10,
            string search = "",
            int? categoryId = null,
            int? vendorId = null,
            string productType = "",
            string stockStatus = "")
        {
            // Base query — include navigations needed for display and filtering
            var baseQuery = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Vendor)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .AsQueryable();

            // Search by SKU or Product.Title
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                baseQuery = baseQuery.Where(v =>
                    v.SKU.ToLower().Contains(q) ||
                    v.Product!.Title.ToLower().Contains(q));
            }

            // Filter by category
            if (categoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product!.ProductCategoryID == categoryId.Value);

            // Filter by vendor
            if (vendorId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product!.VendorID == vendorId.Value);

            // Filter by product type (TypeName string match)
            if (!string.IsNullOrWhiteSpace(productType))
                baseQuery = baseQuery.Where(v => v.Product!.ProductType!.TypeName == productType);

            // Filter by stock level
            baseQuery = stockStatus switch
            {
                "instock"    => baseQuery.Where(v => v.InventoryQuantity > 5),
                "lowstock"   => baseQuery.Where(v => v.InventoryQuantity >= 1 && v.InventoryQuantity <= 5),
                "outofstock" => baseQuery.Where(v => v.InventoryQuantity == 0),
                _            => baseQuery
            };

            // Status counts (across all statuses, respecting search/category/vendor/stock filters)
            ViewBag.AllCount       = await baseQuery.CountAsync();
            ViewBag.PublishedCount = await baseQuery.CountAsync(v => v.Status == ProductStatus.Published);
            ViewBag.DraftCount     = await baseQuery.CountAsync(v => v.Status == ProductStatus.Draft);
            ViewBag.ArchivedCount  = await baseQuery.CountAsync(v => v.Status == ProductStatus.Archived);

            // Apply status filter
            var filteredQuery = filter switch
            {
                "published" => baseQuery.Where(v => v.Status == ProductStatus.Published),
                "draft"     => baseQuery.Where(v => v.Status == ProductStatus.Draft),
                "unlisted"  => baseQuery.Where(v => v.Status == ProductStatus.Unlisted),
                "archived"  => baseQuery.Where(v => v.Status == ProductStatus.Archived),
                _           => baseQuery
            };

            filteredQuery = filteredQuery.OrderBy(v => v.Product!.Title).ThenBy(v => v.SKU);

            // Dropdowns
            ViewBag.Categories = new SelectList(
                await _context.ProductCategories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", categoryId);

            ViewBag.Vendors = new SelectList(
                await _context.Vendors.Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync(),
                "VendorID", "VendorName", vendorId);

            ViewBag.ProductTypes = new SelectList(
                await _context.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.TypeName).ToListAsync(),
                "TypeName", "TypeName", productType);

            // Preserve filter state for view
            ViewBag.CurrentFilter      = filter;
            ViewBag.CurrentSearch      = search;
            ViewBag.CurrentCategoryId  = categoryId;
            ViewBag.CurrentVendorId    = vendorId;
            ViewBag.CurrentProductType = productType;
            ViewBag.CurrentStockStatus = stockStatus;
            ViewBag.CurrentPageSize    = pageSize;

            return View(await PaginatedList<ProductVariant>.CreateAsync(filteredQuery, pageNumber, pageSize));
        }

        // GET: ProductVariants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Vendor)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductType)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductSpecifications)
                .FirstOrDefaultAsync(v => v.VariantID == id);

            if (variant == null) return NotFound();

            return View(variant);
        }

        // GET: ProductVariants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // GET: ProductVariants/Create
        public IActionResult Create(bool fromReview = false)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateProductViewModel viewModel)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // GET: ProductVariants/Review
        public IActionResult Review()
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/ConfirmCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCreate(string saveAction)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // GET: ProductVariants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel viewModel)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Publish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: ProductVariants/Unpublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // ===================================================================
        // AJAX UNIQUENESS CHECK ENDPOINTS
        // ===================================================================

        [HttpGet]
        public async Task<IActionResult> CheckSku(string sku, int? variantId)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return Json(new { isAvailable = true });
            }

            var exists = await _context.ProductVariants
                .AnyAsync(v => v.SKU == sku && v.VariantID != (variantId ?? 0));

            return Json(new { isAvailable = !exists });
        }

        [HttpGet]
        public IActionResult CheckTitle(string title, int? variantId)
        {
            // Title lives on Product in V3, not on variant — always available at variant level
            return Json(new { isAvailable = true });
        }

        // ===================================================================
        // PRIVATE HELPERS
        // ===================================================================

        private List<string> GetProductTypes()
        {
            return new List<string>
            {
                "Chassis Components",
                "Engine Parts",
                "Safety Equipment",
                "Accessories",
                "Tools",
                "Hardware",
                "Cleaning Supplies",
                "Performance Parts",
                "Measurement Tools",
                "Hand Tools",
                "Other - Please Specify"
            };
        }

        private void PopulateProductTypesDropdown(string? currentType = null)
        {
            var productTypes = GetProductTypes();

            if (!string.IsNullOrEmpty(currentType) && !productTypes.Contains(currentType))
            {
                ViewData["ProductTypes"] = new SelectList(productTypes, "Other - Please Specify");
                ViewData["CustomType"] = currentType;
            }
            else
            {
                ViewData["ProductTypes"] = new SelectList(productTypes, currentType);
            }
        }

        private void ReplaceBindingErrorsWithRequiredMessages()
        {
            var fieldMessages = new Dictionary<string, string>
            {
                { "Variant.Price", "Price is required for product creation." },
                { "Variant.InventoryQuantity", "Inventory Quantity is required for product creation." },
                { "Variant.CompareAtPrice", "Compare At Price must be a positive value." },
                { "Variant.Weight", "Weight must be a valid number." }
            };

            foreach (var kvp in fieldMessages)
            {
                if (ModelState.TryGetValue(kvp.Key, out var entry) && entry.Errors.Count > 0)
                {
                    var hasBindingError = entry.Errors.Any(e =>
                        e.ErrorMessage.Contains("The value") && e.ErrorMessage.Contains("is not valid") ||
                        e.ErrorMessage.Contains("The value ''") ||
                        e.ErrorMessage.Contains("The value \"\""));

                    if (hasBindingError)
                    {
                        ModelState.Remove(kvp.Key);
                        ModelState.AddModelError(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        private string GenerateHandle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            string handle = title.ToLower();
            handle = handle.Replace(" ", "-");
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "[^a-z0-9-]", "");
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "-+", "-");
            handle = handle.Trim('-');

            return handle;
        }
    }
}
