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
            string stockStatus = "",
            string sortBy = "created",
            string sortDir = "desc")
        {
            var baseQuery = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Vendor)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductCategory)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.ProductType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                baseQuery = baseQuery.Where(v =>
                    v.SKU.ToLower().Contains(q) ||
                    v.Product!.Title.ToLower().Contains(q));
            }

            if (categoryId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product!.ProductCategoryID == categoryId.Value);

            if (vendorId.HasValue)
                baseQuery = baseQuery.Where(v => v.Product!.VendorID == vendorId.Value);

            if (!string.IsNullOrWhiteSpace(productType))
                baseQuery = baseQuery.Where(v => v.Product!.ProductType!.TypeName == productType);

            baseQuery = stockStatus switch
            {
                "instock"    => baseQuery.Where(v => v.InventoryQuantity > 5),
                "lowstock"   => baseQuery.Where(v => v.InventoryQuantity >= 1 && v.InventoryQuantity <= 5),
                "outofstock" => baseQuery.Where(v => v.InventoryQuantity == 0),
                _            => baseQuery
            };

            ViewBag.AllCount       = await baseQuery.CountAsync();
            ViewBag.PublishedCount = await baseQuery.CountAsync(v => v.Status == ProductStatus.Published);
            ViewBag.DraftCount     = await baseQuery.CountAsync(v => v.Status == ProductStatus.Draft);
            ViewBag.ArchivedCount  = await baseQuery.CountAsync(v => v.Status == ProductStatus.Archived);

            var filteredQuery = filter switch
            {
                "published" => baseQuery.Where(v => v.Status == ProductStatus.Published),
                "draft"     => baseQuery.Where(v => v.Status == ProductStatus.Draft),
                "unlisted"  => baseQuery.Where(v => v.Status == ProductStatus.Unlisted),
                "archived"  => baseQuery.Where(v => v.Status == ProductStatus.Archived),
                _           => baseQuery
            };

            bool asc = sortDir == "asc";
            filteredQuery = sortBy switch
            {
                "title"  => asc ? filteredQuery.OrderBy(v => v.Product!.Title).ThenBy(v => v.VariantID)
                                : filteredQuery.OrderByDescending(v => v.Product!.Title).ThenBy(v => v.VariantID),
                "price"  => asc ? filteredQuery.OrderBy(v => (double)v.Price).ThenBy(v => v.VariantID)
                                : filteredQuery.OrderByDescending(v => (double)v.Price).ThenBy(v => v.VariantID),
                "stock"  => asc ? filteredQuery.OrderBy(v => v.InventoryQuantity).ThenBy(v => v.VariantID)
                                : filteredQuery.OrderByDescending(v => v.InventoryQuantity).ThenBy(v => v.VariantID),
                "status" => asc ? filteredQuery.OrderBy(v => v.Status).ThenBy(v => v.VariantID)
                                : filteredQuery.OrderByDescending(v => v.Status).ThenBy(v => v.VariantID),
                _        => asc ? filteredQuery.OrderBy(v => v.Product!.CreatedDate).ThenBy(v => v.VariantID)
                                : filteredQuery.OrderByDescending(v => v.Product!.CreatedDate).ThenBy(v => v.VariantID),
            };

            ViewData["SortBy"]  = sortBy;
            ViewData["SortDir"] = sortDir;
            ViewData["NextDir"] = sortDir == "asc" ? "desc" : "asc";

            ViewBag.Categories = new SelectList(
                await _context.ProductCategories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", categoryId);

            ViewBag.Vendors = new SelectList(
                await _context.Vendors.Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync(),
                "VendorID", "VendorName", vendorId);

            ViewBag.ProductTypes = new SelectList(
                await _context.ProductTypes.Where(t => t.IsActive).OrderBy(t => t.TypeName).ToListAsync(),
                "TypeName", "TypeName", productType);

            ViewBag.CurrentFilter      = filter;
            ViewBag.CurrentSearch      = search;
            ViewBag.CurrentCategoryId  = categoryId;
            ViewBag.CurrentVendorId    = vendorId;
            ViewBag.CurrentProductType = productType;
            ViewBag.CurrentStockStatus = stockStatus;
            ViewBag.CurrentPageSize    = pageSize;
            ViewBag.CurrentSortBy      = sortBy;
            ViewBag.CurrentSortDir     = sortDir;

            return View(await PaginatedList<ProductVariant>.CreateAsync(filteredQuery, pageNumber, pageSize));
        }

        // GET: ProductVariants/Details/{productId}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Vendor)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductType)
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductSpecifications)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: ProductVariants/Delete/5  (confirmation page — routes by VariantID)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.VariantID == id);
            if (variant == null) return NotFound();
            return View(variant);
        }

        // POST: ProductVariants/Delete/5  (soft-delete single variant)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant != null)
            {
                variant.Status = ProductStatus.Archived;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Variant archived.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: ProductVariants/ArchiveProduct/{productId}  — archives all variants
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            product.Status = ProductStatus.Archived;
            foreach (var v in product.ProductVariants)
                v.Status = ProductStatus.Archived;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product archived.";
            return RedirectToAction(nameof(Index));
        }

        // POST: ProductVariants/RestoreProduct/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            product.Status = ProductStatus.Draft;
            foreach (var v in product.ProductVariants)
                v.Status = ProductStatus.Draft;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product restored to Draft.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ProductVariants/PublishProduct/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            product.Status = ProductStatus.Published;
            foreach (var v in product.ProductVariants.Where(v => v.Status != ProductStatus.Archived))
                v.Status = ProductStatus.Published;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product published.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ProductVariants/UnpublishProduct/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnpublishProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            product.Status = ProductStatus.Draft;
            foreach (var v in product.ProductVariants.Where(v => v.Status != ProductStatus.Archived))
                v.Status = ProductStatus.Draft;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product unpublished (set to Draft).";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: ProductVariants/Restore/5  (Index restore by VariantID — kept for compatibility)
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

        // POST: ProductVariants/Publish/5  (Index publish by VariantID — kept for compatibility)
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

        // POST: ProductVariants/Unpublish/5  (Index unpublish by VariantID — kept for compatibility)
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
            model.ProductTitle      = model.ProductTitle?.Trim() ?? "";
            model.SKU               = model.SKU?.Trim() ?? "";
            model.Description       = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.SimpleOptionName  = string.IsNullOrWhiteSpace(model.SimpleOptionName)  ? null : model.SimpleOptionName.Trim();
            model.SimpleOptionValue = string.IsNullOrWhiteSpace(model.SimpleOptionValue) ? null : model.SimpleOptionValue.Trim();
            model.Barcode           = string.IsNullOrWhiteSpace(model.Barcode)           ? null : model.Barcode.Trim();
            model.Option1Name       = string.IsNullOrWhiteSpace(model.Option1Name) ? null : model.Option1Name.Trim();
            model.Option2Name       = string.IsNullOrWhiteSpace(model.Option2Name) ? null : model.Option2Name.Trim();
            model.Option3Name       = string.IsNullOrWhiteSpace(model.Option3Name) ? null : model.Option3Name.Trim();

            if (model.Variants != null)
                foreach (var v in model.Variants)
                {
                    v.SKU          = v.SKU?.Trim() ?? "";
                    v.Barcode      = string.IsNullOrWhiteSpace(v.Barcode) ? null : v.Barcode.Trim();
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

            // ── 3. Business-rule validation ────────────────────────────────────────

            var trimmedTitle = model.ProductTitle;
            if (!string.IsNullOrWhiteSpace(trimmedTitle))
            {
                if (await _context.Products.AnyAsync(p => EF.Functions.Like(p.Title, trimmedTitle)))
                    ModelState.AddModelError("ProductTitle",
                        "A product with this name already exists. Use a different title.");
            }

            if (!model.HasVariants)
            {
                var trimmedSku = model.SKU;
                if (!string.IsNullOrWhiteSpace(trimmedSku))
                {
                    if (await _context.ProductVariants.AnyAsync(v => EF.Functions.Like(v.SKU, trimmedSku)))
                        ModelState.AddModelError("SKU",
                            "This SKU is already taken. Each product needs a unique SKU.");
                }

                if (!string.IsNullOrWhiteSpace(model.Barcode))
                {
                    if (await _context.ProductVariants.AnyAsync(v =>
                            v.Barcode != null && EF.Functions.Like(v.Barcode, model.Barcode)))
                        ModelState.AddModelError("Barcode",
                            "This barcode is already assigned to another product.");
                }

                if (model.Price <= 0)
                    ModelState.AddModelError("Price", "Price must be greater than zero.");

                if (model.InventoryQuantity < 0)
                    ModelState.AddModelError("InventoryQuantity", "Stock quantity cannot be negative.");

                if (!string.IsNullOrWhiteSpace(model.SimpleOptionName) && string.IsNullOrWhiteSpace(model.SimpleOptionValue))
                    ModelState.AddModelError("SimpleOptionValue",
                        "Enter a value for the selected option, or set Option to None.");
            }
            else
            {
                if (model.Variants == null || !model.Variants.Any())
                {
                    ModelState.AddModelError("", "Add at least one variant before saving.");
                }
                else
                {
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

                    var skuGroups = model.Variants
                        .Select((v, i) => new { Sku = v.SKU.ToUpperInvariant(), Index = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .GroupBy(x => x.Sku)
                        .Where(g => g.Count() > 1);

                    foreach (var group in skuGroups)
                        foreach (var item in group)
                            ModelState.AddModelError($"Variants[{item.Index}].SKU",
                                "This SKU is already used in another row.");

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

                    var barcodeGroups = model.Variants
                        .Select((v, i) => new { v.Barcode, Index = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
                        .GroupBy(x => x.Barcode!.ToUpperInvariant())
                        .Where(g => g.Count() > 1);

                    foreach (var group in barcodeGroups)
                        foreach (var item in group)
                            ModelState.AddModelError($"Variants[{item.Index}].Barcode",
                                "This barcode is already used in another row.");

                    var nonBlankBarcodes = model.Variants
                        .Select((v, i) => new { v.Barcode, Index = i })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
                        .ToList();

                    if (nonBlankBarcodes.Any())
                    {
                        var bcList = nonBlankBarcodes.Select(x => x.Barcode!).Distinct().ToList();
                        var takenBarcodes = await _context.ProductVariants
                            .Where(v => v.Barcode != null && bcList.Contains(v.Barcode))
                            .Select(v => v.Barcode!)
                            .ToListAsync();

                        foreach (var item in nonBlankBarcodes)
                            if (takenBarcodes.Contains(item.Barcode!))
                                ModelState.AddModelError($"Variants[{item.Index}].Barcode",
                                    "This barcode is already assigned to another product.");
                    }

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

            if (!ModelState.IsValid)
            {
                await PopulateEditDropdowns(model.VendorID, model.ProductCategoryID, model.ProductTypeID);
                return View(model);
            }

            // ── 4. Save product ────────────────────────────────────────────────────
            var createdBy = User.Identity?.Name ?? "system";

            var product = new Product
            {
                Title             = model.ProductTitle,
                Handle            = GenerateHandle(model.ProductTitle),
                Description       = model.Description,
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
                        Barcode           = string.IsNullOrWhiteSpace(row.Barcode) ? null : row.Barcode,
                        Price             = row.Price,
                        CostPrice         = row.CostPrice,
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
                    Barcode           = string.IsNullOrWhiteSpace(model.Barcode) ? null : model.Barcode,
                    Price             = model.Price,
                    CostPrice         = model.CostPrice,
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

            return RedirectToAction(nameof(Details), new { id = product.ProductID });
        }

        // GET: ProductVariants/Edit/{productId}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.ProductSpecifications)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            var activeVariants = product.ProductVariants
                .Where(v => v.Status != ProductStatus.Archived)
                .OrderBy(v => v.VariantID)
                .ToList();

            bool isSimple = activeVariants.Count == 1 && activeVariants[0].Option1Name == "Title";

            var vm = new EditProductViewModel
            {
                ProductID         = product.ProductID,
                Title             = product.Title,
                Description       = product.Description,
                VendorID          = product.VendorID,
                ProductCategoryID = product.ProductCategoryID,
                ProductTypeID     = product.ProductTypeID,
                Status            = product.Status,
                IsSimpleProduct   = isSimple,
                Specifications    = product.ProductSpecifications
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new EditSpecificationRowViewModel
                    {
                        SpecID       = s.SpecificationID,
                        Label        = s.Name,
                        Value        = s.Value,
                        DisplayOrder = s.DisplayOrder
                    }).ToList()
            };

            if (isSimple)
            {
                var v = activeVariants[0];
                vm.SimpleVariantID   = v.VariantID;
                vm.SKU               = v.SKU;
                vm.Price             = v.Price;
                vm.CostPrice         = v.CostPrice;
                vm.CompareAtPrice    = v.CompareAtPrice;
                vm.InventoryQuantity = v.InventoryQuantity;
                vm.Weight            = v.Weight;
                vm.Barcode           = v.Barcode;
                // Expose as simple option only when name is not the "Title" sentinel
                vm.SimpleOptionName  = v.Option1Name == "Title" ? null : v.Option1Name;
                vm.SimpleOptionValue = v.Option1Name == "Title" ? null : v.Option1Value;
            }
            else
            {
                var first = activeVariants.FirstOrDefault();
                vm.Option1Name      = first?.Option1Name;
                vm.Option2Name      = first?.Option2Name;
                vm.Option3Name      = first?.Option3Name;
                vm.DefaultVariantID = activeVariants.FirstOrDefault(v => v.IsDefault)?.VariantID
                                      ?? activeVariants.FirstOrDefault()?.VariantID ?? 0;
                vm.Variants = activeVariants.Select(v => new EditVariantRowViewModel
                {
                    VariantID         = v.VariantID,
                    SKU               = v.SKU,
                    Price             = v.Price,
                    CostPrice         = v.CostPrice,
                    InventoryQuantity = v.InventoryQuantity,
                    Barcode           = v.Barcode,
                    Option1Value      = v.Option1Value,
                    Option2Value      = v.Option2Value,
                    Option3Value      = v.Option3Value
                }).ToList();
            }

            await PopulateEditDropdowns(vm.VendorID, vm.ProductCategoryID, vm.ProductTypeID);
            return View(vm);
        }

        // POST: ProductVariants/Edit/{productId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditProductViewModel viewModel, string? saveAction = null)
        {
            if (id != viewModel.ProductID) return NotFound();

            // ── 1. Trim inputs ─────────────────────────────────────────────────────
            viewModel.Title             = viewModel.Title?.Trim() ?? "";
            viewModel.Description       = string.IsNullOrWhiteSpace(viewModel.Description) ? null : viewModel.Description.Trim();
            viewModel.SKU               = viewModel.SKU?.Trim() ?? "";
            viewModel.Barcode           = string.IsNullOrWhiteSpace(viewModel.Barcode) ? null : viewModel.Barcode.Trim();
            viewModel.SimpleOptionName  = string.IsNullOrWhiteSpace(viewModel.SimpleOptionName) ? null : viewModel.SimpleOptionName.Trim();
            viewModel.SimpleOptionValue = string.IsNullOrWhiteSpace(viewModel.SimpleOptionValue) ? null : viewModel.SimpleOptionValue.Trim();
            viewModel.Option1Name       = string.IsNullOrWhiteSpace(viewModel.Option1Name) ? null : viewModel.Option1Name.Trim();
            viewModel.Option2Name       = string.IsNullOrWhiteSpace(viewModel.Option2Name) ? null : viewModel.Option2Name.Trim();
            viewModel.Option3Name       = string.IsNullOrWhiteSpace(viewModel.Option3Name) ? null : viewModel.Option3Name.Trim();

            if (viewModel.Variants != null)
                foreach (var v in viewModel.Variants)
                {
                    v.SKU     = v.SKU?.Trim() ?? "";
                    v.Barcode = string.IsNullOrWhiteSpace(v.Barcode) ? null : v.Barcode.Trim();
                }

            if (viewModel.Specifications != null)
                foreach (var s in viewModel.Specifications)
                {
                    s.Label = s.Label?.Trim() ?? "";
                    s.Value = s.Value?.Trim() ?? "";
                }

            // ── 2. Remove irrelevant-path model errors ─────────────────────────────
            if (viewModel.IsSimpleProduct)
            {
                ModelState.Remove("Option1Name");
                ModelState.Remove("Option2Name");
                ModelState.Remove("Option3Name");
            }
            else
            {
                ModelState.Remove("SKU");
                ModelState.Remove("Price");
                ModelState.Remove("InventoryQuantity");
            }

            // ── 3. Business-rule validation ────────────────────────────────────────

            if (!string.IsNullOrWhiteSpace(viewModel.Title))
            {
                if (await _context.Products.AnyAsync(p =>
                        p.ProductID != viewModel.ProductID &&
                        EF.Functions.Like(p.Title, viewModel.Title)))
                    ModelState.AddModelError("Title",
                        "A product with this name already exists. Use a different title.");
            }

            if (viewModel.IsSimpleProduct)
            {
                if (!string.IsNullOrWhiteSpace(viewModel.SKU))
                {
                    if (await _context.ProductVariants.AnyAsync(v =>
                            v.VariantID != viewModel.SimpleVariantID &&
                            EF.Functions.Like(v.SKU, viewModel.SKU)))
                        ModelState.AddModelError("SKU",
                            "This SKU is already taken. Each product needs a unique SKU.");
                }

                if (viewModel.Price <= 0)
                    ModelState.AddModelError("Price", "Price must be greater than zero.");

                if (viewModel.InventoryQuantity < 0)
                    ModelState.AddModelError("InventoryQuantity", "Stock quantity cannot be negative.");

                if (!string.IsNullOrWhiteSpace(viewModel.Barcode))
                {
                    if (await _context.ProductVariants.AnyAsync(v =>
                            v.VariantID != viewModel.SimpleVariantID &&
                            v.Barcode != null &&
                            EF.Functions.Like(v.Barcode, viewModel.Barcode)))
                        ModelState.AddModelError("Barcode",
                            "This barcode is already assigned to another product.");
                }

                if (!string.IsNullOrWhiteSpace(viewModel.SimpleOptionName) &&
                    string.IsNullOrWhiteSpace(viewModel.SimpleOptionValue))
                    ModelState.AddModelError("SimpleOptionValue",
                        "Enter a value for the selected option, or set Option to None.");
            }
            else
            {
                var activeRows = viewModel.Variants?.Where(v => !v.IsArchived).ToList()
                                 ?? new List<EditVariantRowViewModel>();

                if (!activeRows.Any())
                {
                    ModelState.AddModelError("", "At least one active variant is required.");
                }
                else
                {
                    for (int i = 0; i < (viewModel.Variants?.Count ?? 0); i++)
                    {
                        var row = viewModel.Variants![i];
                        if (row.IsArchived) continue;

                        if (string.IsNullOrWhiteSpace(row.SKU))
                            ModelState.AddModelError($"Variants[{i}].SKU",
                                "SKU is required for each variant.");

                        if (row.Price <= 0)
                            ModelState.AddModelError($"Variants[{i}].Price",
                                "Price must be greater than zero.");

                        if (row.InventoryQuantity < 0)
                            ModelState.AddModelError($"Variants[{i}].InventoryQuantity",
                                "Stock quantity cannot be negative.");
                    }

                    // Intra-submission duplicate SKUs (non-archived only)
                    var skuGroups = viewModel.Variants!
                        .Select((v, i) => new { Sku = v.SKU.ToUpperInvariant(), Index = i, v.IsArchived })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku) && !x.IsArchived)
                        .GroupBy(x => x.Sku)
                        .Where(g => g.Count() > 1);

                    foreach (var group in skuGroups)
                        foreach (var item in group)
                            ModelState.AddModelError($"Variants[{item.Index}].SKU",
                                "This SKU is already used in another row.");

                    // DB duplicate SKUs (exclude current variant IDs)
                    var currentVariantIds = viewModel.Variants!
                        .Where(v => v.VariantID > 0)
                        .Select(v => v.VariantID)
                        .ToList();

                    var nonArchivedSkus = viewModel.Variants!
                        .Select((v, i) => new { Sku = v.SKU.ToUpperInvariant(), v.VariantID, Index = i, v.IsArchived })
                        .Where(x => !x.IsArchived && !string.IsNullOrWhiteSpace(x.Sku))
                        .ToList();

                    if (nonArchivedSkus.Any())
                    {
                        var skuList   = nonArchivedSkus.Select(x => x.Sku).Distinct().ToList();
                        var takenSkus = await _context.ProductVariants
                            .Where(v => skuList.Contains(v.SKU) && !currentVariantIds.Contains(v.VariantID))
                            .Select(v => v.SKU)
                            .ToListAsync();

                        foreach (var item in nonArchivedSkus)
                            if (takenSkus.Contains(item.Sku))
                                ModelState.AddModelError($"Variants[{item.Index}].SKU",
                                    "This SKU is already taken. Each product needs a unique SKU.");
                    }

                    // Intra-submission duplicate barcodes
                    var barcodeGroups = viewModel.Variants!
                        .Select((v, i) => new { v.Barcode, Index = i, v.IsArchived })
                        .Where(x => !string.IsNullOrWhiteSpace(x.Barcode) && !x.IsArchived)
                        .GroupBy(x => x.Barcode!.ToUpperInvariant())
                        .Where(g => g.Count() > 1);

                    foreach (var group in barcodeGroups)
                        foreach (var item in group)
                            ModelState.AddModelError($"Variants[{item.Index}].Barcode",
                                "This barcode is already used in another row.");
                }
            }

            if (viewModel.Specifications != null)
            {
                bool addedSpecError = false;
                foreach (var spec in viewModel.Specifications)
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

            if (!ModelState.IsValid)
            {
                await PopulateEditDropdowns(viewModel.VendorID, viewModel.ProductCategoryID, viewModel.ProductTypeID);
                return View(viewModel);
            }

            // ── 4. Persist changes ─────────────────────────────────────────────────
            var modifiedBy = User.Identity?.Name ?? "system";

            var product = await _context.Products.FindAsync(viewModel.ProductID);
            if (product == null) return NotFound();

            product.Title             = viewModel.Title;
            product.Handle            = GenerateHandle(viewModel.Title);
            product.Description       = viewModel.Description;
            product.VendorID          = viewModel.VendorID;
            product.ProductCategoryID = viewModel.ProductCategoryID;
            product.ProductTypeID     = viewModel.ProductTypeID;
            product.Status            = viewModel.Status;
            product.ModifiedDate      = DateTime.UtcNow;
            product.ModifiedBy        = modifiedBy;

            if (viewModel.IsSimpleProduct)
            {
                var variant = await _context.ProductVariants.FindAsync(viewModel.SimpleVariantID);
                if (variant != null)
                {
                    bool hasOption = !string.IsNullOrWhiteSpace(viewModel.SimpleOptionName) &&
                                     !string.IsNullOrWhiteSpace(viewModel.SimpleOptionValue);
                    variant.SKU               = viewModel.SKU.ToUpperInvariant();
                    variant.Price             = viewModel.Price;
                    variant.CostPrice         = viewModel.CostPrice;
                    variant.CompareAtPrice    = viewModel.CompareAtPrice;
                    variant.InventoryQuantity = viewModel.InventoryQuantity;
                    variant.Weight            = viewModel.Weight;
                    variant.Barcode           = viewModel.Barcode;
                    variant.Status            = viewModel.Status;
                    variant.Option1Name       = hasOption ? viewModel.SimpleOptionName : "Title";
                    variant.Option1Value      = hasOption ? viewModel.SimpleOptionValue : "Default Title";
                    variant.IsDefault         = !hasOption;
                    variant.ModifiedDate      = DateTime.UtcNow;
                    variant.ModifiedBy        = modifiedBy;
                }
            }
            else
            {
                // ── Update existing variants ───────────────────────────────────
                foreach (var row in viewModel.Variants!)
                {
                    if (row.VariantID <= 0) continue;

                    var variant = await _context.ProductVariants.FindAsync(row.VariantID);
                    if (variant == null) continue;

                    if (row.IsArchived)
                    {
                        variant.Status       = ProductStatus.Archived;
                        variant.IsDefault    = false;
                        variant.ModifiedDate = DateTime.UtcNow;
                        variant.ModifiedBy   = modifiedBy;
                    }
                    else
                    {
                        variant.SKU               = row.SKU.ToUpperInvariant();
                        variant.Price             = row.Price;
                        variant.CostPrice         = row.CostPrice;
                        variant.InventoryQuantity = row.InventoryQuantity;
                        variant.Barcode           = row.Barcode;
                        variant.Option1Name       = viewModel.Option1Name;
                        variant.Option2Name       = viewModel.Option2Name;
                        variant.Option3Name       = viewModel.Option3Name;
                        variant.IsDefault         = row.VariantID == viewModel.DefaultVariantID;
                        variant.Status            = viewModel.Status;
                        variant.ModifiedDate      = DateTime.UtcNow;
                        variant.ModifiedBy        = modifiedBy;
                    }
                }

                // ── Insert new variants (VariantID == 0) ──────────────────────
                foreach (var row in viewModel.Variants!.Where(r => r.VariantID <= 0 && !r.IsArchived))
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductID         = product.ProductID,
                        SKU               = row.SKU.ToUpperInvariant(),
                        Barcode           = string.IsNullOrWhiteSpace(row.Barcode) ? null : row.Barcode,
                        Price             = row.Price,
                        CostPrice         = row.CostPrice,
                        InventoryQuantity = row.InventoryQuantity,
                        Option1Name       = string.IsNullOrWhiteSpace(viewModel.Option1Name) ? null : viewModel.Option1Name,
                        Option1Value      = string.IsNullOrWhiteSpace(row.Option1Value) ? null : row.Option1Value,
                        Option2Name       = string.IsNullOrWhiteSpace(viewModel.Option2Name) ? null : viewModel.Option2Name,
                        Option2Value      = string.IsNullOrWhiteSpace(viewModel.Option2Name) ? null : (string.IsNullOrWhiteSpace(row.Option2Value) ? null : row.Option2Value),
                        Option3Name       = string.IsNullOrWhiteSpace(viewModel.Option3Name) ? null : viewModel.Option3Name,
                        Option3Value      = string.IsNullOrWhiteSpace(viewModel.Option3Name) ? null : (string.IsNullOrWhiteSpace(row.Option3Value) ? null : row.Option3Value),
                        IsDefault         = false,
                        Status            = viewModel.Status,
                        InventoryPolicy   = "deny",
                        RequiresShipping  = true,
                        IsTaxable         = true,
                        CreatedDate       = DateTime.UtcNow,
                        CreatedBy         = modifiedBy
                    });
                }
            }

            // Replace all specifications
            var oldSpecs = await _context.ProductSpecifications
                .Where(s => s.ProductID == viewModel.ProductID)
                .ToListAsync();
            _context.ProductSpecifications.RemoveRange(oldSpecs);

            if (viewModel.Specifications != null)
            {
                int order = 0;
                foreach (var spec in viewModel.Specifications.Where(s => !string.IsNullOrWhiteSpace(s.Label)))
                {
                    _context.ProductSpecifications.Add(new ProductSpecification
                    {
                        ProductID    = viewModel.ProductID,
                        Name         = spec.Label,
                        Value        = spec.Value,
                        DisplayOrder = order++
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Ensure exactly one default variant (auto-promote if needed)
            if (!viewModel.IsSimpleProduct)
            {
                var allActiveVariants = await _context.ProductVariants
                    .Where(v => v.ProductID == viewModel.ProductID && v.Status != ProductStatus.Archived)
                    .OrderBy(v => v.VariantID)
                    .ToListAsync();

                if (allActiveVariants.Any() && !allActiveVariants.Any(v => v.IsDefault))
                {
                    allActiveVariants.First().IsDefault = true;
                    await _context.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Product updated.";

            return RedirectToAction(nameof(Details), new { id = viewModel.ProductID });
        }

        // ===================================================================
        // AJAX UNIQUENESS CHECK ENDPOINTS
        // ===================================================================

        [HttpGet]
        public async Task<IActionResult> CheckSku(string sku, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return Json(new { available = true });

            var exists = await _context.ProductVariants
                .AnyAsync(v => v.VariantID != excludeId && EF.Functions.Like(v.SKU, sku.Trim()));

            return Json(new { available = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTitle(string title, int excludeId = 0)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Json(new { available = true });

            var exists = await _context.Products
                .AnyAsync(p => p.ProductID != excludeId && EF.Functions.Like(p.Title, title.Trim()));

            return Json(new { available = !exists });
        }

        [HttpGet]
        public async Task<IActionResult> CheckBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return Json(new { available = true });

            var trimmed = barcode.Trim();
            var exists = await _context.ProductVariants
                .AnyAsync(v => v.Barcode != null && EF.Functions.Like(v.Barcode, trimmed));

            return Json(new { available = !exists });
        }

        // ===================================================================
        // PRIVATE HELPERS
        // ===================================================================

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

        private string GenerateHandle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            string handle = title.ToLower();
            handle = handle.Replace(" ", "-");
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "[^a-z0-9-]", "");
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "-+", "-");
            handle = handle.Trim('-');
            return handle;
        }
    }
}
