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
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductType)
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
            if (id == null) return NotFound();
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantID == id);
            if (variant == null) return NotFound();
            return View(variant);
        }

        // POST: ProductVariants/Delete/5  (soft-delete → Archived)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant != null)
            {
                variant.Status = ProductStatus.Archived;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product archived.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ProductVariants/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant != null)
            {
                variant.Status = ProductStatus.Published;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product restored.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: ProductVariants/Create
        public async Task<IActionResult> Create()
        {
            await PopulateEditDropdowns(0, 0, null);
            return View(new QuickCreateViewModel());
        }

        // POST: ProductVariants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuickCreateViewModel model)
        {
            // ── 1. Trim all string inputs ──────────────────────────────────────────
            model.ProductTitle = model.ProductTitle?.Trim() ?? "";
            model.SKU          = model.SKU?.Trim() ?? "";
            model.SimpleOptionName  = string.IsNullOrWhiteSpace(model.SimpleOptionName)  ? null : model.SimpleOptionName.Trim();
            model.SimpleOptionValue = string.IsNullOrWhiteSpace(model.SimpleOptionValue) ? null : model.SimpleOptionValue.Trim();
            model.Option1Name  = string.IsNullOrWhiteSpace(model.Option1Name) ? null : model.Option1Name.Trim();
            model.Option2Name  = string.IsNullOrWhiteSpace(model.Option2Name) ? null : model.Option2Name.Trim();
            model.Option3Name  = string.IsNullOrWhiteSpace(model.Option3Name) ? null : model.Option3Name.Trim();

            if (model.Variants != null)
                foreach (var v in model.Variants)
                {
                    v.SKU          = v.SKU?.Trim() ?? "";
                    v.Option1Value = string.IsNullOrWhiteSpace(v.Option1Value) ? null : v.Option1Value.Trim();
                    v.Option2Value = string.IsNullOrWhiteSpace(v.Option2Value) ? null : v.Option2Value.Trim();
                    v.Option3Value = string.IsNullOrWhiteSpace(v.Option3Value) ? null : v.Option3Value.Trim();
                }

            if (model.Specifications != null)
                foreach (var s in model.Specifications)
                {
                    s.Label = s.Label?.Trim() ?? "";
                    s.Value = s.Value?.Trim() ?? "";
                }

            // ── 2. Suppress Required errors for the inactive path ──────────────────
            if (model.HasVariants)
            {
                ModelState.Remove("SKU");
                ModelState.Remove("Price");
                ModelState.Remove("InventoryQuantity");
            }

            // ── 3. Business-rule validation (title/SKU duplicates, price, inventory) ──

            // Title duplicate — case-insensitive via EF.Functions.Like
            var trimmedTitle = model.ProductTitle;
            if (!string.IsNullOrWhiteSpace(trimmedTitle))
            {
                if (await _context.Products.AnyAsync(p => EF.Functions.Like(p.Title, trimmedTitle)))
                    ModelState.AddModelError("ProductTitle",
                        "A product with this name already exists. Use a different title.");
            }

            if (!model.HasVariants)
            {
                // SKU duplicate — simple path, case-insensitive via EF.Functions.Like
                var trimmedSku = model.SKU;
                if (!string.IsNullOrWhiteSpace(trimmedSku))
                {
                    if (await _context.ProductVariants.AnyAsync(v => EF.Functions.Like(v.SKU, trimmedSku)))
                        ModelState.AddModelError("SKU",
                            "This SKU is already taken. Each product needs a unique SKU.");
                }

                // Price > 0
                if (model.Price <= 0)
                    ModelState.AddModelError("Price", "Price must be greater than zero.");

                // Negative inventory
                if (model.InventoryQuantity < 0)
                    ModelState.AddModelError("InventoryQuantity", "Stock quantity cannot be negative.");

                // Simple option: name selected but value blank
                if (!string.IsNullOrWhiteSpace(model.SimpleOptionName) && string.IsNullOrWhiteSpace(model.SimpleOptionValue))
                    ModelState.AddModelError("SimpleOptionValue",
                        "Enter a value for the selected option, or set Option to None.");
            }
            else
            {
                // HasVariants = true
                if (model.Variants == null || !model.Variants.Any())
                {
                    ModelState.AddModelError("", "Add at least one variant before saving.");
                }
                else
                {
                    // Per-row validations
                    for (int i = 0; i < model.Variants.Count; i++)
                    {
                        var row = model.Variants[i];

                        if (string.IsNullOrWhiteSpace(row.SKU))
                            ModelState.AddModelError($"Variants[{i}].SKU",
                                "SKU is required for each variant.");

                        if (row.Price <= 0)
                            ModelState.AddModelError($"Variants[{i}].Price",
                                "Price must be greater than zero.");

                        if (row.Qty < 0)
                            ModelState.AddModelError($"Variants[{i}].Qty",
                                "Stock quantity cannot be negative.");
                    }

                    // Intra-submission duplicate SKUs — flag ALL rows, not just first
                    var skuGroups = model.Variants
                        .Select((v, i) => new { Sku = v.SKU.ToUpperInvariant(), Index = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .GroupBy(x => x.Sku)
                        .Where(g => g.Count() > 1);

                    foreach (var group in skuGroups)
                        foreach (var item in group)
                            ModelState.AddModelError($"Variants[{item.Index}].SKU",
                                "This SKU is already used in another row.");

                    // DB duplicate SKUs — collect ALL, not just first
                    var nonBlankSkus = model.Variants
                        .Select((v, i) => new { Sku = v.SKU.ToUpperInvariant(), Index = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .ToList();

                    if (nonBlankSkus.Any())
                    {
                        var skuList   = nonBlankSkus.Select(x => x.Sku).Distinct().ToList();
                        var takenSkus = await _context.ProductVariants
                            .Where(v => skuList.Contains(v.SKU))
                            .Select(v => v.SKU)
                            .ToListAsync();

                        foreach (var item in nonBlankSkus)
                            if (takenSkus.Contains(item.Sku))
                                ModelState.AddModelError($"Variants[{item.Index}].SKU",
                                    "This SKU is already taken. Each product needs a unique SKU.");
                    }

                    // Option name set but no variant carries a value for it
                    if (!string.IsNullOrWhiteSpace(model.Option1Name) &&
                        !model.Variants.Any(v => !string.IsNullOrWhiteSpace(v.Option1Value)))
                        ModelState.AddModelError("",
                            $"Option '{model.Option1Name}' has no values. Add at least one value or remove the option.");

                    if (!string.IsNullOrWhiteSpace(model.Option2Name) &&
                        !model.Variants.Any(v => !string.IsNullOrWhiteSpace(v.Option2Value)))
                        ModelState.AddModelError("",
                            $"Option '{model.Option2Name}' has no values. Add at least one value or remove the option.");

                    if (!string.IsNullOrWhiteSpace(model.Option3Name) &&
                        !model.Variants.Any(v => !string.IsNullOrWhiteSpace(v.Option3Value)))
                        ModelState.AddModelError("",
                            $"Option '{model.Option3Name}' has no values. Add at least one value or remove the option.");
                }
            }

            // Specification validation — blank Label with non-blank Value
            if (model.Specifications != null)
            {
                bool addedSpecError = false;
                foreach (var spec in model.Specifications)
                {
                    if (!string.IsNullOrWhiteSpace(spec.Value) && string.IsNullOrWhiteSpace(spec.Label))
                    {
                        if (!addedSpecError)
                        {
                            ModelState.AddModelError("",
                                "Each specification needs a label. Remove the row or add a label.");
                            addedSpecError = true;
                        }
                    }
                }
            }

            // ── 4. Return early if any validation failed ───────────────────────────
            if (!ModelState.IsValid)
            {
                await PopulateEditDropdowns(model.VendorID, model.ProductCategoryID, model.ProductTypeID);
                return View(model);
            }

            // ── 5. Save product ────────────────────────────────────────────────────
            var createdBy = User.Identity?.Name ?? "system";

            var product = new Product
            {
                Title             = model.ProductTitle,
                Handle            = GenerateHandle(model.ProductTitle),
                VendorID          = model.VendorID,
                ProductCategoryID = model.ProductCategoryID,
                ProductTypeID     = model.ProductTypeID,
                Status            = model.Status,
                CreatedDate       = DateTime.UtcNow,
                CreatedBy         = createdBy
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (model.HasVariants)
            {
                bool isFirst = true;
                foreach (var row in model.Variants!)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductID         = product.ProductID,
                        SKU               = row.SKU.ToUpperInvariant(),
                        Price             = row.Price,
                        InventoryQuantity = row.Qty,
                        InventoryPolicy   = "deny",
                        RequiresShipping  = true,
                        IsTaxable         = true,
                        Status            = model.Status,
                        IsDefault         = isFirst,
                        Option1Name       = string.IsNullOrWhiteSpace(model.Option1Name) ? null : model.Option1Name,
                        Option1Value      = row.Option1Value,
                        Option2Name       = string.IsNullOrWhiteSpace(model.Option2Name) ? null : model.Option2Name,
                        Option2Value      = string.IsNullOrWhiteSpace(model.Option2Name) ? null : row.Option2Value,
                        Option3Name       = string.IsNullOrWhiteSpace(model.Option3Name) ? null : model.Option3Name,
                        Option3Value      = string.IsNullOrWhiteSpace(model.Option3Name) ? null : row.Option3Value,
                        CreatedDate       = DateTime.UtcNow,
                        CreatedBy         = createdBy
                    });
                    isFirst = false;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Product \"{product.Title}\" created with {model.Variants.Count} variant(s).";
            }
            else
            {
                bool hasOption = !string.IsNullOrWhiteSpace(model.SimpleOptionName) &&
                                 !string.IsNullOrWhiteSpace(model.SimpleOptionValue);
                _context.ProductVariants.Add(new ProductVariant
                {
                    ProductID         = product.ProductID,
                    SKU               = model.SKU.ToUpperInvariant(),
                    Price             = model.Price,
                    InventoryQuantity = model.InventoryQuantity,
                    InventoryPolicy   = "deny",
                    RequiresShipping  = true,
                    IsTaxable         = true,
                    Status            = model.Status,
                    IsDefault         = !hasOption,
                    Option1Name       = hasOption ? model.SimpleOptionName : "Title",
                    Option1Value      = hasOption ? model.SimpleOptionValue : "Default Title",
                    CreatedDate       = DateTime.UtcNow,
                    CreatedBy         = createdBy
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Product \"{product.Title}\" created.";
            }

            // ── 6. Save specifications ─────────────────────────────────────────────
            if (model.Specifications != null)
            {
                var validSpecs = model.Specifications
                    .Where(s => !string.IsNullOrWhiteSpace(s.Label))
                    .ToList();

                if (validSpecs.Any())
                {
                    int order = 0;
                    foreach (var spec in validSpecs)
                    {
                        _context.ProductSpecifications.Add(new ProductSpecification
                        {
                            ProductID    = product.ProductID,
                            Name         = spec.Label,
                            Value        = spec.Value,
                            DisplayOrder = order++
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ProductVariants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductSpecifications)
                .FirstOrDefaultAsync(v => v.VariantID == id);

            if (variant == null) return NotFound();

            var viewModel = new CreateProductViewModel
            {
                Product = variant.Product!,
                Variant = variant,
                Attributes = variant.Product!.ProductSpecifications
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new AttributeInputModel
                    {
                        AttributeName = s.Name,
                        AttributeValue = s.Value,
                        DisplayOrder = s.DisplayOrder
                    })
                    .ToList()
            };

            await PopulateEditDropdowns(viewModel.Product.VendorID, viewModel.Product.ProductCategoryID, viewModel.Product.ProductTypeID);
            return View(viewModel);
        }

        // POST: ProductVariants/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel viewModel)
        {
            if (id != viewModel.Variant.VariantID) return NotFound();

            // Handle is generated — remove its validation error
            ModelState.Remove("Product.Handle");
            ReplaceBindingErrorsWithRequiredMessages();

            if (!ModelState.IsValid)
            {
                await PopulateEditDropdowns(viewModel.Product.VendorID, viewModel.Product.ProductCategoryID, viewModel.Product.ProductTypeID);
                return View(viewModel);
            }

            var existingVariant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantID == id);

            if (existingVariant == null) return NotFound();

            var existingProduct = existingVariant.Product!;

            // Update Product
            existingProduct.Title = viewModel.Product.Title;
            existingProduct.Handle = GenerateHandle(viewModel.Product.Title);
            existingProduct.VendorID = viewModel.Product.VendorID;
            existingProduct.ProductCategoryID = viewModel.Product.ProductCategoryID;
            existingProduct.ProductTypeID = viewModel.Product.ProductTypeID;
            existingProduct.Description = viewModel.Product.Description;
            existingProduct.Tags = viewModel.Product.Tags;
            existingProduct.ModifiedDate = DateTime.UtcNow;

            // Update Variant
            existingVariant.SKU = viewModel.Variant.SKU;
            existingVariant.Price = viewModel.Variant.Price;
            existingVariant.CompareAtPrice = viewModel.Variant.CompareAtPrice;
            existingVariant.InventoryQuantity = viewModel.Variant.InventoryQuantity;
            existingVariant.InventoryPolicy = viewModel.Variant.InventoryPolicy;
            existingVariant.Weight = viewModel.Variant.Weight;
            existingVariant.Barcode = viewModel.Variant.Barcode;
            existingVariant.RequiresShipping = viewModel.Variant.RequiresShipping;
            existingVariant.IsTaxable = viewModel.Variant.IsTaxable;
            existingVariant.ModifiedDate = DateTime.UtcNow;

            // Replace specifications
            var oldSpecs = await _context.ProductSpecifications
                .Where(s => s.ProductID == existingProduct.ProductID)
                .ToListAsync();
            _context.ProductSpecifications.RemoveRange(oldSpecs);

            foreach (var attr in viewModel.Attributes)
            {
                if (!string.IsNullOrWhiteSpace(attr.AttributeName) && !string.IsNullOrWhiteSpace(attr.AttributeValue))
                {
                    _context.ProductSpecifications.Add(new ProductSpecification
                    {
                        ProductID = existingProduct.ProductID,
                        Name = attr.AttributeName,
                        Value = attr.AttributeValue,
                        DisplayOrder = attr.DisplayOrder
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ProductVariants/Publish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();

            variant.Status = ProductStatus.Published;
            variant.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product published.";
            return RedirectToAction(nameof(Index));
        }

        // POST: ProductVariants/Unpublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();

            variant.Status = ProductStatus.Draft;
            variant.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product unpublished (set to Draft).";
            return RedirectToAction(nameof(Index));
        }

        // ===================================================================
        // AJAX UNIQUENESS CHECK ENDPOINTS
        // ===================================================================

        [HttpGet]
        public async Task<IActionResult> CheckSku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return Json(new { available = true });

            var exists = await _context.ProductVariants
                .AnyAsync(v => EF.Functions.Like(v.SKU, sku.Trim()));

            return Json(new { available = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Json(new { available = true });

            var exists = await _context.Products
                .AnyAsync(p => EF.Functions.Like(p.Title, title.Trim()));

            return Json(new { available = !exists });
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

        private async Task PopulateEditDropdowns(int vendorId, int categoryId, int? productTypeId)
        {
            ViewBag.VendorID = new SelectList(
                await _context.Vendors.Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync(),
                "VendorID", "VendorName", vendorId);

            ViewBag.ProductCategoryID = new SelectList(
                await _context.ProductCategories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", categoryId);

            ViewData["ProductTypes"] = new SelectList(
                await _context.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.TypeName).ToListAsync(),
                "ProductTypeID", "TypeName", productTypeId);
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
