using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> Index(string filter = "all")
			{
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

			var variants = await query.ToListAsync();

			// Pass filter to view for highlighting active filter
			ViewBag.CurrentFilter = filter;

			return View(variants);
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
			return View(viewModel);
			}

		// POST: ProductVariants/Create
		// POST: ProductVariants/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreateProductViewModel viewModel)
			{
			var variant = viewModel.Variant;

			// Generate Handle from Title FIRST
			if (!string.IsNullOrWhiteSpace(variant.Title))
				{
				variant.Handle = GenerateHandle(variant.Title);
				}
			else
				{
				variant.Handle = string.Empty;
				}

			// Remove Handle from validation (it's auto-generated)
			ModelState.Remove("Variant.Handle");

			// DEBUG: Show validation errors if any
			if (!ModelState.IsValid)
				{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
				return View(viewModel);
				}

			try
				{
				// Set as standalone product (no parent)
				variant.ProductID = null;

				// Set initial status
				variant.Status = ProductStatus.Draft;
				variant.IsActive = false;

				// Save variant first
				_context.Add(variant);
				await _context.SaveChangesAsync();

				// Now save attributes with the VariantID
				if (viewModel.Attributes != null && viewModel.Attributes.Any())
					{
					foreach (var attrInput in viewModel.Attributes)
						{
						// Only save if both name and value are provided
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
				// Get the actual error message
				var innerException = ex.InnerException?.Message ?? ex.Message;

				// List to collect all errors
				var errorMessages = new List<string>();

				// Check for SKU duplicate
				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.SKU"))
					{
					errorMessages.Add($"SKU '{variant.SKU}' already exists.");
					ModelState.AddModelError("Variant.SKU", "This SKU is already in use.");
					}

				// Check for Handle duplicate
				if (innerException.Contains("UNIQUE constraint failed: ProductVariants.Handle"))
					{
					errorMessages.Add($"A product with a similar title already exists (Handle: '{variant.Handle}').");
					ModelState.AddModelError("Variant.Title", "A product with this title already exists.");
					}

				// If we found specific errors, show them
				if (errorMessages.Any())
					{
					TempData["Error"] = "❌ " + string.Join(" ", errorMessages);
					}
				else
					{
					// Generic database error
					TempData["Error"] = $"❌ Database error: {innerException}";
					}

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
				.Include(v => v.Attributes.OrderBy(a => a.DisplayOrder))
				.FirstOrDefaultAsync(v => v.VariantID == id);

			if (variant == null)
				{
				return NotFound();
				}

			// Create view model and populate with existing data
			var viewModel = new CreateProductViewModel
				{
				Variant = variant,
				Attributes = variant.Attributes.Select(a => new AttributeInputModel
					{
					AttributeName = a.AttributeName,
					AttributeValue = a.AttributeValue,
					IsVariantAttribute = a.IsVariantAttribute,
					DisplayOrder = a.DisplayOrder
					}).ToList()
				};

			return View(viewModel);
			}

		// POST: ProductVariants/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, CreateProductViewModel viewModel)
			{
			var variant = viewModel.Variant;

			if (id != variant.VariantID)
				{
				return NotFound();
				}

			// Generate Handle from Title
			if (!string.IsNullOrWhiteSpace(variant.Title))
				{
				variant.Handle = GenerateHandle(variant.Title);
				}

			// Remove Handle from validation
			ModelState.Remove("Variant.Handle");

			if (!ModelState.IsValid)
				{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
				return View(viewModel);
				}

			try
				{
				// Get existing variant from database
				var existingVariant = await _context.ProductVariants
					.Include(v => v.Attributes)
					.FirstOrDefaultAsync(v => v.VariantID == id);

				if (existingVariant == null)
					{
					return NotFound();
					}

				// Update variant properties
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

				// Handle attributes: Remove all existing, add new ones
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
					errorMessages.Add($"A product with a similar title already exists (Handle: '{variant.Handle}').");
					ModelState.AddModelError("Variant.Title", "A product with this title already exists.");
					}

				if (errorMessages.Any())
					{
					TempData["Error"] = "❌ " + string.Join(" ", errorMessages);
					}
				else
					{
					TempData["Error"] = $"❌ Database error: {innerException}";
					}

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