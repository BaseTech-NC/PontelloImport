using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using System.Text.RegularExpressions;

namespace PontelloImport.Controllers
	{
	public class ProductsController : Controller
		{
		private readonly PontelloDbContext _context;

		public ProductsController(PontelloDbContext context)
			{
			_context = context;
			}

		// GET: Products
		public async Task<IActionResult> Index()
			{
			var products = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.Include(p => p.Variants)
				.Where(p => p.IsActive)
				.OrderBy(p => p.Title)
				.ToListAsync();

			return View(products);
			}

		// GET: Products/Details/5
		public async Task<IActionResult> Details(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var product = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.Include(p => p.Variants)
					.ThenInclude(v => v.Attributes)
				.FirstOrDefaultAsync(m => m.ProductID == id);

			if (product == null)
				{
				return NotFound();
				}

			return View(product);
			}

		// GET: Products/Create
		public IActionResult Create()
			{
			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName");
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName");
			return View();
			}

		// POST: Products/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Title,Description,Type,Tags,VendorID,ProductCategoryID")] Product product)
			{
			if (ModelState.IsValid)
				{
				// Auto-generate Handle
				product.Handle = GenerateHandle(product.Title);

				// Ensure Handle is unique
				product.Handle = await EnsureUniqueHandle(product.Handle);

				product.IsActive = true;

				_context.Add(product);
				await _context.SaveChangesAsync();

				TempData["Success"] = $"Parent product '{product.Title}' created successfully!";
				return RedirectToAction(nameof(Details), new { id = product.ProductID });
				}

			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
			return View(product);
			}

		// GET: Products/Edit/5
		public async Task<IActionResult> Edit(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var product = await _context.Products.FindAsync(id);
			if (product == null)
				{
				return NotFound();
				}

			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
			return View(product);
			}

		// POST: Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ProductID,Title,Handle,BodyHTML,Type,Tags,VendorID,ProductCategoryID,IsActive")] Product product)
			{
			if (id != product.ProductID)
				{
				return NotFound();
				}

			if (ModelState.IsValid)
				{
				try
					{
					_context.Update(product);
					await _context.SaveChangesAsync();

					TempData["Success"] = $"Product '{product.Title}' updated successfully!";
					}
				catch (DbUpdateConcurrencyException)
					{
					if (!ProductExists(product.ProductID))
						{
						return NotFound();
						}
					else
						{
						throw;
						}
					}
				return RedirectToAction(nameof(Details), new { id = product.ProductID });
				}

			ViewData["VendorID"] = new SelectList(_context.Vendors.Where(v => v.IsActive), "VendorID", "VendorName", product.VendorID);
			ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.ProductCategoryID);
			return View(product);
			}

		// GET: Products/Delete/5
		public async Task<IActionResult> Delete(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var product = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.Include(p => p.Variants)
				.FirstOrDefaultAsync(m => m.ProductID == id);

			if (product == null)
				{
				return NotFound();
				}

			return View(product);
			}

		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
			{
			var product = await _context.Products
				.Include(p => p.Variants)
				.FirstOrDefaultAsync(p => p.ProductID == id);

			if (product != null)
				{
				// Check if product has variants
				if (product.Variants.Any())
					{
					TempData["Error"] = $"Cannot delete '{product.Title}' because it has {product.Variants.Count} variant(s). Delete variants first.";
					return RedirectToAction(nameof(Details), new { id = product.ProductID });
					}

				// Soft delete
				product.IsActive = false;
				_context.Update(product);
				await _context.SaveChangesAsync();

				TempData["Success"] = $"Product '{product.Title}' archived successfully!";
				}

			return RedirectToAction(nameof(Index));
			}

		private bool ProductExists(int id)
			{
			return _context.Products.Any(e => e.ProductID == id);
			}

		// Helper: Generate Handle from Title
		private string GenerateHandle(string title)
			{
			if (string.IsNullOrWhiteSpace(title))
				return string.Empty;

			// Convert to lowercase
			string handle = title.ToLowerInvariant();

			// Replace special characters
			handle = handle
				.Replace("/", "-")
				.Replace("°", "")
				.Replace("\"", "")
				.Replace("'", "")
				.Replace("(", "")
				.Replace(")", "")
				.Replace("[", "")
				.Replace("]", "")
				.Replace("&", "and")
				.Replace("½", "1-2")
				.Replace("¼", "1-4")
				.Replace("¾", "3-4");

			// Replace spaces with hyphens
			handle = handle.Replace(" ", "-");

			// Remove multiple consecutive hyphens
			while (handle.Contains("--"))
				{
				handle = handle.Replace("--", "-");
				}

			// Remove non-alphanumeric except hyphens
			handle = Regex.Replace(handle, @"[^a-z0-9\-]", "");

			// Trim hyphens
			handle = handle.Trim('-');

			return handle;
			}

		// Helper: Ensure Handle is Unique
		private async Task<string> EnsureUniqueHandle(string baseHandle)
			{
			string handle = baseHandle;
			int suffix = 1;

			while (await _context.Products.AnyAsync(p => p.Handle == handle))
				{
				handle = $"{baseHandle}-{suffix}";
				suffix++;
				}

			return handle;
			}
		}
	}