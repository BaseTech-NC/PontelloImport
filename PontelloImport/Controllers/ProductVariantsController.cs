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


        // GET: ProductVariants - NOW WITH PAGINATION
        public async Task<IActionResult> Index(string filter = "all", int pageNumber = 1, int pageSize = 10)
        {
            // Validate page size (prevent abuse)
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // Base query - get all variants with related data
            IQueryable<ProductVariant> query = _context.ProductVariants
                .Include(v => v.Product)                    // Include parent product (if exists)
                    .ThenInclude(p => p.Vendor)            // Include vendor through parent
                .Include(v => v.Product.ProductCategory)   // Include category through parent
                .Include(v => v.Attributes)                // Include attributes
                .OrderByDescending(v => v.CreatedDate);    // Newest first

            // Apply filters
            switch (filter.ToLower())
            {
                case "standalone":
                    query = query.Where(v => v.ProductID == null);  // No parent = standalone
                    break;
                case "variants":
                    query = query.Where(v => v.ProductID != null);  // Has parent = variant
                    break;
                case "published":
                    query = query.Where(v => v.Status == ProductStatus.Published);
                    break;
                case "draft":
                    query = query.Where(v => v.Status == ProductStatus.Draft);
                    break;
                case "archived":
                    query = query.Where(v => v.Status == ProductStatus.Archived);
                    break;
                    // "all" = no additional filter
            }

            // REMOVE THIS LINE:
            // var variants = await query.ToListAsync();

            // Store filter counts for the filter buttons (calculate before pagination)
            var allVariants = await _context.ProductVariants.ToListAsync();
            ViewBag.AllCount = allVariants.Count;
            ViewBag.StandaloneCount = allVariants.Count(v => v.ProductID == null);
            ViewBag.VariantsCount = allVariants.Count(v => v.ProductID != null);
            ViewBag.PublishedCount = allVariants.Count(v => v.Status == ProductStatus.Published);
            ViewBag.DraftCount = allVariants.Count(v => v.Status == ProductStatus.Draft);
            ViewBag.ArchivedCount = allVariants.Count(v => v.Status == ProductStatus.Archived);

            // Pass filter and page size to view
            ViewBag.CurrentFilter = filter;
            ViewBag.CurrentPageSize = pageSize;

            // CREATE PAGINATED LIST - ADD THIS LINE:
            var paginatedVariants = await PaginatedList<ProductVariant>.CreateAsync(query, pageNumber, pageSize);

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

            TempData["Success"] = $"Variant '{variant.Title}' has been archived successfully.";

            return RedirectToAction(nameof(Index));
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

            TempData["Success"] = $"Variant '{variant.Title}' restored successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: ProductVariants/Create
        public IActionResult Create()
        {
            var viewModel = new CreateProductViewModel();

			// NEW: Add dropdown data for Vendor and Category
			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName");
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName");

			return View(viewModel);
        }


		// POST: ProductVariants/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateProductViewModel viewModel)
			{
			var product = viewModel.Product;
			var variant = viewModel.Variant;

			// Generate Handles from Titles
			// For simple products, copy Variant.Title to Product.Title
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

			// For simple products, copy Variant.Title to Product.Title
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

			// Remove Product.Title from validation (auto-copied from Variant.Title)
			ModelState.Remove("Product.Title");

			// Remove Handles from validation (auto-generated)
			ModelState.Remove("Product.Handle");
			ModelState.Remove("Variant.Handle");

			if (!ModelState.IsValid)
				{
				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);

				return View(viewModel);
				}

			try
				{
				// STEP 1: Create Product parent FIRST
				product.Status = ProductStatus.Draft;
				product.IsActive = true;

				_context.Products.Add(product);
				await _context.SaveChangesAsync();  // Save to get ProductID

				// STEP 2: Create ProductVariant child (linked to parent)
				variant.ProductID = product.ProductID;  // Link to parent
				variant.Status = ProductStatus.Draft;
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

				TempData["Success"] = $"Product '{variant.Title}' created successfully with {viewModel.Attributes?.Count(a => !string.IsNullOrWhiteSpace(a.AttributeName))} attribute(s)!";
				return RedirectToAction(nameof(Details), new { id = variant.VariantID });
				}
			catch (DbUpdateException ex)
				{
				var innerException = ex.InnerException?.Message ?? ex.Message;
				var errorMessages = new List<string>();

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.SKU"))
					{
					errorMessages.Add($"SKU '{variant.SKU}' already exists.");
					ModelState.AddModelError("Variant.SKU", "This SKU is already in use.");
					}

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.Handle"))
					{
					errorMessages.Add($"A product with a similar title already exists (Handle: '{variant.Handle}').");
					ModelState.AddModelError("Variant.Title", "A product with this title already exists.");
					}

				if (innerException.Contains("UNIQUE constraint failed: Products.Handle"))
					{
					errorMessages.Add($"A product with a similar title already exists (Handle: '{product.Handle}').");
					ModelState.AddModelError("Product.Title", "A product with this title already exists.");
					}

				if (errorMessages.Any())
					{
					TempData["Error"] = "❌ " + string.Join(" ", errorMessages);
					}
				else
					{
					TempData["Error"] = $"❌ Database error: {innerException}";
					}

				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);

				return View(viewModel);
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

			if (!ModelState.IsValid)
				{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				TempData["Error"] = "Validation failed: " + string.Join(", ", errors);

				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);

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
					errorMessages.Add($"SKU '{variant.SKU}' already exists.");
					ModelState.AddModelError("Variant.SKU", "This SKU is already in use.");
					}

				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.Handle"))
					{
					errorMessages.Add($"A product with a similar title already exists.");
					ModelState.AddModelError("Variant.Title", "A product with this title already exists.");
					}

				if (innerException.Contains("UNIQUE constraint failed: Products.Handle"))
					{
					errorMessages.Add($"A product with a similar title already exists.");
					ModelState.AddModelError("Product.Title", "A product with this title already exists.");
					}

				if (errorMessages.Any())
					{
					TempData["Error"] = string.Join(" ", errorMessages);
					}
				else
					{
					TempData["Error"] = $"Database error: {innerException}";
					}

				// Re-populate dropdowns on error
				ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
				ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);

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