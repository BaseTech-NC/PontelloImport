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


        // GET: ProductVariants - Search, Filter & Pagination
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
            // Validate page size (prevent abuse)
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Sanitize inputs
            search = search?.Trim() ?? "";
            productType = productType?.Trim() ?? "";
            stockStatus = stockStatus?.Trim() ?? "";

            // Base query with related data
            IQueryable<ProductVariant> query = _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Vendor)
                .Include(v => v.Product.ProductCategory)
                .Include(v => v.Attributes);

            // Search filter (Title or SKU)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v =>
                    v.Title.ToLower().Contains(search.ToLower()) ||
                    v.SKU.ToLower().Contains(search.ToLower()));
            }

            // Category filter
            if (categoryId.HasValue)
            {
                query = query.Where(v => v.Product != null && v.Product.ProductCategoryID == categoryId.Value);
            }

            // Vendor filter
            if (vendorId.HasValue)
            {
                query = query.Where(v => v.Product != null && v.Product.VendorID == vendorId.Value);
            }

            // Product Type filter
            if (!string.IsNullOrEmpty(productType))
            {
                query = query.Where(v => v.Product != null && v.Product.Type == productType);
            }

            // Stock Status filter
            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus.ToLower())
                {
                    case "instock":
                        query = query.Where(v => v.InventoryQuantity > 5);
                        break;
                    case "lowstock":
                        query = query.Where(v => v.InventoryQuantity >= 1 && v.InventoryQuantity <= 5);
                        break;
                    case "outofstock":
                        query = query.Where(v => v.InventoryQuantity == 0);
                        break;
                }
            }

            // Count queries BEFORE applying status filter (counts reflect search + dropdown filters)
            ViewBag.AllCount = await query.Where(v => v.Status != ProductStatus.Archived).CountAsync();
            ViewBag.PublishedCount = await query.Where(v => v.Status == ProductStatus.Published).CountAsync();
            ViewBag.DraftCount = await query.Where(v => v.Status == ProductStatus.Draft).CountAsync();
            ViewBag.ArchivedCount = await query.Where(v => v.Status == ProductStatus.Archived).CountAsync();

            // Apply status tab filter
            switch (filter.ToLower())
            {
                case "published":
                    query = query.Where(v => v.Status == ProductStatus.Published);
                    break;
                case "draft":
                    query = query.Where(v => v.Status == ProductStatus.Draft);
                    break;
                case "archived":
                    query = query.Where(v => v.Status == ProductStatus.Archived);
                    break;
                default: // "all" — show non-archived
                    query = query.Where(v => v.Status != ProductStatus.Archived);
                    break;
            }

            // Order and paginate
            query = query.OrderByDescending(v => v.CreatedDate);
            var paginatedVariants = await PaginatedList<ProductVariant>.CreateAsync(query, pageNumber, pageSize);

            // Populate dropdown data
            ViewBag.Categories = new SelectList(
                await _context.ProductCategories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", categoryId);

            ViewBag.Vendors = new SelectList(
                await _context.Vendors.Where(v => v.IsActive).OrderBy(v => v.VendorName).ToListAsync(),
                "VendorID", "VendorName", vendorId);

            ViewBag.ProductTypes = new SelectList(
                await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.Type))
                    .Select(p => p.Type)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),
                productType);

            // Pass all filter state to view
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentPageSize = pageSize;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentVendorId = vendorId;
            ViewBag.CurrentProductType = productType;
            ViewBag.CurrentStockStatus = stockStatus;

            return View(paginatedVariants);
        }

        // GET: ProductVariants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variant = await _context.ProductVariants
                .Include(v => v.Product)                    // Parent product
                    .ThenInclude(p => p.Vendor)            // Vendor through parent
                .Include(v => v.Product.ProductCategory)   // Category through parent
                .Include(v => v.Attributes.OrderBy(a => a.DisplayOrder))  // Attributes ordered
                .FirstOrDefaultAsync(v => v.VariantID == id);

            if (variant == null)
            {
                return NotFound();
            }

            return View(variant);
        }

        // GET: ProductVariants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p.Vendor)
                .Include(v => v.Attributes)
                .FirstOrDefaultAsync(v => v.VariantID == id);

            if (variant == null)
            {
                return NotFound();
            }

            return View(variant);
        }

        // POST: ProductVariants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);

            if (variant == null)
            {
                return NotFound();
            }

            // SOFT DELETE - Don't actually remove from database
            variant.Status = ProductStatus.Archived;
            variant.IsActive = false;

            _context.Update(variant);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{variant.Title}' has been archived successfully.";

            return RedirectToAction(nameof(Details), new { id = variant.VariantID });
        }




        // POST: ProductVariants/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);

            if (variant == null)
            {
                TempData["Error"] = "Variant not found.";
                return RedirectToAction(nameof(Index));
            }

            // Simple restore: Archived → Draft
            variant.Status = ProductStatus.Draft;
            variant.IsActive = true;

            _context.Update(variant);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{variant.Title}' has been restored as a draft.";

            return RedirectToAction(nameof(Details), new { id = variant.VariantID });
        }

        // GET: ProductVariants/Create
        public IActionResult Create(bool fromReview = false)
        {
            CreateProductViewModel viewModel;

			// Only load TempData when returning from Review page via "Back to Edit"
			if (fromReview && TempData.ContainsKey(ReviewTempDataKey))
				{
				var json = TempData[ReviewTempDataKey] as string;
				// Do NOT call TempData.Keep - let it be consumed so fresh visits start clean

				if (!string.IsNullOrEmpty(json))
					{
					viewModel = JsonSerializer.Deserialize<CreateProductViewModel>(json)
								?? new CreateProductViewModel();
					}
				else
					{
					viewModel = new CreateProductViewModel();
					}
				}
			else
				{
				// Fresh visit - clear any stale review data
				if (TempData.ContainsKey(ReviewTempDataKey))
					{
					TempData.Remove(ReviewTempDataKey);
					}
				viewModel = new CreateProductViewModel();
				}

			// Add dropdown data for Vendor, Category, and Product Type
			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", viewModel.Product.VendorID);
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", viewModel.Product.ProductCategoryID);
			PopulateProductTypesDropdown(viewModel.Product.Type);

			return View(viewModel);
        }


		// POST: ProductVariants/Create - Validate and redirect to Review (no DB save)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(CreateProductViewModel viewModel)
			{
			var product = viewModel.Product;
			var variant = viewModel.Variant;

			// For simple products, copy Variant.Title to Product.Title
			if (string.IsNullOrWhiteSpace(product.Title) && !string.IsNullOrWhiteSpace(variant.Title))
				{
				product.Title = variant.Title;
				}

			// Remove Product.Title from validation (auto-copied from Variant.Title)
			ModelState.Remove("Product.Title");

			// Remove Handles from validation (auto-generated at save time)
			ModelState.Remove("Product.Handle");
			ModelState.Remove("Variant.Handle");

			// Replace generic "The value '' is invalid." with friendly required messages
			ReplaceBindingErrorsWithRequiredMessages();

			if (!ModelState.IsValid)
				{
				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
				PopulateProductTypesDropdown(product.Type);

				return View(viewModel);
				}

			// Generate handles for display on Review page
			if (!string.IsNullOrWhiteSpace(product.Title))
				{
				product.Handle = GenerateHandle(product.Title);
				}
			if (!string.IsNullOrWhiteSpace(variant.Title))
				{
				variant.Handle = GenerateHandle(variant.Title);
				}

			// Serialize to TempData and redirect to Review page
			var json = JsonSerializer.Serialize(viewModel);
			TempData[ReviewTempDataKey] = json;

			return RedirectToAction(nameof(Review));
			}


		// GET: ProductVariants/Review - Display product data for confirmation
		public IActionResult Review()
			{
			if (!TempData.ContainsKey(ReviewTempDataKey))
				{
				TempData["Error"] = "No product data to review. Please fill out the form first.";
				return RedirectToAction(nameof(Create));
				}

			var json = TempData[ReviewTempDataKey] as string;
			TempData.Keep(ReviewTempDataKey); // Keep data for Save or Back to Edit

			if (string.IsNullOrEmpty(json))
				{
				TempData["Error"] = "Product data was lost. Please fill out the form again.";
				return RedirectToAction(nameof(Create));
				}

			var viewModel = JsonSerializer.Deserialize<CreateProductViewModel>(json);

			if (viewModel == null)
				{
				TempData["Error"] = "Could not load product data. Please try again.";
				return RedirectToAction(nameof(Create));
				}

			// Look up Vendor and Category names for display
			if (viewModel.Product.VendorID.HasValue)
				{
				var vendor = _context.Vendors.Find(viewModel.Product.VendorID.Value);
				ViewData["VendorName"] = vendor?.VendorName ?? "Unknown Vendor";
				}

			if (viewModel.Product.ProductCategoryID.HasValue)
				{
				var category = _context.ProductCategories.Find(viewModel.Product.ProductCategoryID.Value);
				ViewData["CategoryName"] = category?.CategoryName ?? "Unknown Category";
				}

			return View(viewModel);
			}


		// POST: ProductVariants/ConfirmCreate - Save product to database
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ConfirmCreate(string saveAction)
			{
			if (!TempData.ContainsKey(ReviewTempDataKey))
				{
				TempData["Error"] = "Product data was lost. Please fill out the form again.";
				return RedirectToAction(nameof(Create));
				}

			var json = TempData[ReviewTempDataKey] as string;
			// Do NOT call TempData.Keep - consume the data after save

			if (string.IsNullOrEmpty(json))
				{
				TempData["Error"] = "Product data was lost. Please fill out the form again.";
				return RedirectToAction(nameof(Create));
				}

			var viewModel = JsonSerializer.Deserialize<CreateProductViewModel>(json);

			if (viewModel == null)
				{
				TempData["Error"] = "Could not load product data. Please try again.";
				return RedirectToAction(nameof(Create));
				}

			var product = viewModel.Product;
			var variant = viewModel.Variant;

			// Regenerate handles fresh
			if (string.IsNullOrWhiteSpace(product.Title) && !string.IsNullOrWhiteSpace(variant.Title))
				{
				product.Title = variant.Title;
				}

			if (!string.IsNullOrWhiteSpace(product.Title))
				{
				product.Handle = GenerateHandle(product.Title);
				}
			if (!string.IsNullOrWhiteSpace(variant.Title))
				{
				variant.Handle = GenerateHandle(variant.Title);
				}

			// Set status based on which button was clicked
			bool isPublish = saveAction == "publish";

			try
				{
				// STEP 1: Create Product parent FIRST
				product.ProductID = 0; // Ensure EF treats as new entity
				product.Status = isPublish ? ProductStatus.Published : ProductStatus.Draft;
				product.IsActive = isPublish;

				_context.Products.Add(product);
				await _context.SaveChangesAsync();

				// STEP 2: Create ProductVariant child (linked to parent)
				variant.VariantID = 0; // Ensure EF treats as new entity
				variant.ProductID = product.ProductID;
				variant.Status = isPublish ? ProductStatus.Published : ProductStatus.Draft;
				variant.IsActive = false;

				_context.ProductVariants.Add(variant);
				await _context.SaveChangesAsync();

				// STEP 3: Save attributes
				if (viewModel.Attributes != null && viewModel.Attributes.Any())
					{
					foreach (var attrInput in viewModel.Attributes)
						{
						if (!string.IsNullOrWhiteSpace(attrInput.AttributeName) &&
							!string.IsNullOrWhiteSpace(attrInput.AttributeValue))
							{
							var attribute = new ProductAttribute
								{
								VariantID = variant.VariantID,
								AttributeName = attrInput.AttributeName.Trim(),
								AttributeValue = attrInput.AttributeValue.Trim(),
								IsVariantAttribute = attrInput.IsVariantAttribute,
								DisplayOrder = attrInput.DisplayOrder
								};

							_context.ProductAttributes.Add(attribute);
							}
						}

					await _context.SaveChangesAsync();
					}

				var statusLabel = isPublish ? "published" : "saved as draft";
				var attrCount = viewModel.Attributes?.Count(a => !string.IsNullOrWhiteSpace(a.AttributeName)) ?? 0;
				TempData["Success"] = $"Product '{variant.Title}' {statusLabel} successfully with {attrCount} attribute(s)!";
				return RedirectToAction(nameof(Details), new { id = variant.VariantID });
				}
			catch (DbUpdateException ex)
				{
				var innerException = ex.InnerException?.Message ?? ex.Message;

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.SKU"))
					{
					TempData["Error"] = $"The SKU '{variant.SKU}' is already in use. Please go back and change it.";
					}
				else if (innerException.Contains("UNIQUE constraint failed: ProductVariants.Handle") ||
						 innerException.Contains("UNIQUE constraint failed: Products.Handle"))
					{
					TempData["Error"] = "A product with this title already exists. Please go back and change the title.";
					}
				else
					{
					TempData["Error"] = $"An unexpected database error occurred: {innerException}";
					}

				// Re-store the data so user can go back and fix
				TempData[ReviewTempDataKey] = json;
				return RedirectToAction(nameof(Review));
				}
			}


		// GET: ProductVariants/Edit/5
		public async Task<IActionResult> Edit(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var variant = await _context.ProductVariants
				.Include(v => v.Product)
				.Include(v => v.Attributes.OrderBy(a => a.DisplayOrder))
				.FirstOrDefaultAsync(v => v.VariantID == id);

			if (variant == null)
				{
				return NotFound("Variant not found.");
				}

			// Load the Product parent
			var product = variant.Product;

			if (product == null)
				{
				return NotFound("Product parent not found.");
				}

			// Create view model and populate with existing data
			var viewModel = new CreateProductViewModel
				{
				Product = product,
				Variant = variant,
				Attributes = variant.Attributes.Select(a => new AttributeInputModel
					{
					AttributeName = a.AttributeName,
					AttributeValue = a.AttributeValue,
					IsVariantAttribute = a.IsVariantAttribute,
					DisplayOrder = a.DisplayOrder
					}).ToList()
				};

			// Add dropdown data
			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
			PopulateProductTypesDropdown(product.Type);

			return View(viewModel);
			}

		// POST: ProductVariants/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, CreateProductViewModel viewModel)
			{
			var product = viewModel.Product;
			var variant = viewModel.Variant;

			if (id != variant.VariantID)
				{
				return NotFound();
				}

			// For simple products, sync titles if Product.Title is empty
			if (string.IsNullOrWhiteSpace(product.Title) && !string.IsNullOrWhiteSpace(variant.Title))
				{
				product.Title = variant.Title;
				}

			// Generate Handles from Titles
			if (!string.IsNullOrWhiteSpace(product.Title))
				{
				product.Handle = GenerateHandle(product.Title);
				}

			if (!string.IsNullOrWhiteSpace(variant.Title))
				{
				variant.Handle = GenerateHandle(variant.Title);
				}

			// Remove from validation
			ModelState.Remove("Product.Title");
			ModelState.Remove("Product.Handle");
			ModelState.Remove("Variant.Handle");

			// Replace generic "The value '' is invalid." with friendly required messages
			ReplaceBindingErrorsWithRequiredMessages();

			if (!ModelState.IsValid)
				{
				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
				PopulateProductTypesDropdown(product.Type);

				return View(viewModel);
				}

			try
				{
				// STEP 1: Update Product parent
				var existingProduct = await _context.Products.FindAsync(variant.ProductID);

				if (existingProduct == null)
					{
					return NotFound("Product parent not found.");
					}

				existingProduct.Title = product.Title;
				existingProduct.Handle = product.Handle;
				existingProduct.VendorID = product.VendorID;
				existingProduct.ProductCategoryID = product.ProductCategoryID;
				existingProduct.Description = product.Description;
				existingProduct.Type = product.Type;
				existingProduct.Tags = product.Tags;
				existingProduct.Status = product.Status;
				existingProduct.IsActive = product.IsActive;

				_context.Update(existingProduct);

				// STEP 2: Update ProductVariant child
				var existingVariant = await _context.ProductVariants
					.Include(v => v.Attributes)
					.FirstOrDefaultAsync(v => v.VariantID == id);

				if (existingVariant == null)
					{
					return NotFound();
					}

				existingVariant.Title = variant.Title;
				existingVariant.Handle = variant.Handle;
				existingVariant.SKU = variant.SKU;
				existingVariant.Price = variant.Price;
				existingVariant.CompareAtPrice = variant.CompareAtPrice;
				existingVariant.InventoryQuantity = variant.InventoryQuantity;
				existingVariant.InventoryPolicy = variant.InventoryPolicy;
				existingVariant.Weight = variant.Weight;
				existingVariant.Barcode = variant.Barcode;
				existingVariant.RequiresShipping = variant.RequiresShipping;
				existingVariant.IsTaxable = variant.IsTaxable;
				existingVariant.Description = variant.Description;
				existingVariant.Type = variant.Type;
				existingVariant.Tags = variant.Tags;
				existingVariant.Status = variant.Status;
				existingVariant.IsActive = variant.IsActive;

				// STEP 3: Handle attributes
				_context.ProductAttributes.RemoveRange(existingVariant.Attributes);

				if (viewModel.Attributes != null && viewModel.Attributes.Any())
					{
					foreach (var attrInput in viewModel.Attributes)
						{
						if (!string.IsNullOrWhiteSpace(attrInput.AttributeName) &&
							!string.IsNullOrWhiteSpace(attrInput.AttributeValue))
							{
							var attribute = new ProductAttribute
								{
								VariantID = existingVariant.VariantID,
								AttributeName = attrInput.AttributeName.Trim(),
								AttributeValue = attrInput.AttributeValue.Trim(),
								IsVariantAttribute = attrInput.IsVariantAttribute,
								DisplayOrder = attrInput.DisplayOrder
								};

							_context.ProductAttributes.Add(attribute);
							}
						}
					}

				await _context.SaveChangesAsync();

				TempData["Success"] = $"Product '{existingVariant.Title}' updated successfully!";
				return RedirectToAction(nameof(Details), new { id = existingVariant.VariantID });
				}
			catch (DbUpdateException ex)
				{
				var innerException = ex.InnerException?.Message ?? ex.Message;
				var errorMessages = new List<string>();

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.SKU"))
					{
					errorMessages.Add($"The SKU '{variant.SKU}' is already in use.");
					ModelState.AddModelError("Variant.SKU", "This SKU is already in use. Please choose a unique SKU.");
					}

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.Handle"))
					{
					errorMessages.Add("A product with this title already exists.");
					ModelState.AddModelError("Variant.Title", "A product with this title already exists. Please use a different title.");
					}

				if (innerException.Contains("UNIQUE constraint failed: Products.Handle"))
					{
					errorMessages.Add("A product with this title already exists.");
					ModelState.AddModelError("Product.Title", "A product with this title already exists. Please use a different title.");
					}

				if (!errorMessages.Any())
					{
					TempData["Error"] = $"An unexpected database error occurred: {innerException}";
					}

				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
				PopulateProductTypesDropdown(product.Type);

				return View(viewModel);
				}
			}

		// POST: ProductVariants/Publish/5
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);

            if (variant == null)
            {
                return NotFound();
            }

            // Change status to Published and activate
            variant.Status = ProductStatus.Published;
            variant.IsActive = true;

            _context.Update(variant);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Product '{variant.Title}' is now published and visible to dealers!";

            return RedirectToAction(nameof(Details), new { id = variant.VariantID });
        }

        // POST: ProductVariants/Unpublish/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unpublish(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);

            if (variant == null)
            {
                return NotFound();
            }

            // Change status to Draft and deactivate
            variant.Status = ProductStatus.Draft;
            variant.IsActive = false;

            _context.Update(variant);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Product '{variant.Title}' is now unpublished and hidden from dealers.";

            return RedirectToAction(nameof(Details), new { id = variant.VariantID });
        }


        // ===================================================================
        // AJAX UNIQUENESS CHECK ENDPOINTS
        // ===================================================================

        /// <summary>
        /// Checks if a SKU is already in use by another variant.
        /// Called via AJAX for instant client-side feedback.
        /// </summary>
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

        /// <summary>
        /// Checks if a product title (via handle) is already in use by another variant.
        /// Called via AJAX for instant client-side feedback.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckTitle(string title, int? variantId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new { isAvailable = true });
            }

            var handle = GenerateHandle(title);
            var exists = await _context.ProductVariants
                .AnyAsync(v => v.Handle == handle && v.VariantID != (variantId ?? 0));

            return Json(new { isAvailable = !exists });
        }

        /// <summary>
        /// Replaces generic "The value '' is invalid." binding errors with friendly required messages.
        /// This happens when a number field (int/decimal) receives an empty string — model binding
        /// fails before [Required] validation ever runs, producing an unhelpful default message.
        /// </summary>
        /// <summary>
        /// Returns the predefined product types list for the dropdown.
        /// </summary>
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

        /// <summary>
        /// Populates ViewData with ProductTypes dropdown, handling custom types for Edit.
        /// </summary>
        private void PopulateProductTypesDropdown(string? currentType = null)
        {
            var productTypes = GetProductTypes();

            if (!string.IsNullOrEmpty(currentType) && !productTypes.Contains(currentType))
            {
                // Current type is custom — select "Other" and store the custom value
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
                    // Check if any error is the generic binding error
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

        // Helper method to generate URL-friendly handle from title
        private string GenerateHandle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Convert to lowercase
            string handle = title.ToLower();

            // Replace spaces with hyphens
            handle = handle.Replace(" ", "-");

            // Remove special characters (keep only letters, numbers, hyphens)
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "[^a-z0-9-]", "");

            // Remove multiple consecutive hyphens
            handle = System.Text.RegularExpressions.Regex.Replace(handle, "-+", "-");

            // Trim hyphens from start and end
            handle = handle.Trim('-');

            return handle;
        }
    }
}