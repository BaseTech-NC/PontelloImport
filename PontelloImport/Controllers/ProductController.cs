using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

//namespace PontelloImport.Controllers
//{
//    public class ProductController : Controller
//    {
//        private readonly PontelloDbContext _context;

//        public ProductController(PontelloDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Product
//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.Product.ToListAsync());
//        }

//        // GET: Product/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var product = await _context.Product
//                .FirstOrDefaultAsync(m => m.ID == id);
//            if (product == null)
//            {
//                return NotFound();
//            }

//            return View(product);
//        }

//        // GET: Product/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: Product/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("ID,Handle,Title,Description,SKU,Name,Price,InventoryQuantity,Type")] Product product)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(product);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(product);
//        }

//        // GET: Product/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var product = await _context.Product.FindAsync(id);
//            if (product == null)
//            {
//                return NotFound();
//            }
//            return View(product);
//        }

//        // POST: Product/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("ID,Handle,Title,Description,SKU,Name,Price,InventoryQuantity,Type")] Product product)
//        {
//            if (id != product.ID)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(product);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!ProductExists(product.ID))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(product);
//        }

//        // GET: Product/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var product = await _context.Product
//                .FirstOrDefaultAsync(m => m.ID == id);
//            if (product == null)
//            {
//                return NotFound();
//            }

//            return View(product);
//        }

//        // POST: Product/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var product = await _context.Product.FindAsync(id);
//            if (product != null)
//            {
//                _context.Product.Remove(product);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool ProductExists(int id)
//        {
//            return _context.Product.Any(e => e.ID == id);
//        }
//    }
//}


namespace PontelloImport.Controllers
	{
	public class ProductController : Controller
		{
		private readonly PontelloDbContext _context;

		public ProductController(PontelloDbContext context)
			{
			_context = context;
			}

		// GET: Product
		public async Task<IActionResult> Index()
			{
			var products = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.Where(p => p.IsActive)
				.OrderBy(p => p.Title)
				.ToListAsync();

			return View(products);
			}

		// GET: Product/Details/5
		public async Task<IActionResult> Details(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var product = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.FirstOrDefaultAsync(m => m.ProductID == id);

			if (product == null)
				{
				return NotFound();
				}

			return View(product);
			}

		// GET: Product/Create
		public IActionResult Create()
			{
			PopulateDropDownLists();
			return View();
			}

		// POST: Product/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Title,BodyHTML,VendorID,ProductCategoryID,Type,Tags,Price,CompareAtPrice,InventoryQuantity,InventoryPolicy,RequiresShipping,IsTaxable,Barcode,Weight")] Product product)
			{
			if (ModelState.IsValid)
				{
				// Auto-generate Handle from Title
				product.Handle = GenerateSlug(product.Title);
				product.Handle = await EnsureUniqueHandle(product.Handle);

				// Auto-generate SKU (temporary - basic version)
				product.SKU = await GenerateSKU();

				// Set defaults
				product.IsActive = true;

				_context.Add(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
				}

			PopulateDropDownLists(product);
			return View(product);
			}

		// GET: Product/Edit/5
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

			PopulateDropDownLists(product);
			return View(product);
			}

		// POST: Product/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ProductID,Handle,SKU,Title,BodyHTML,VendorID,ProductCategoryID,Type,Tags,Price,CompareAtPrice,InventoryQuantity,InventoryPolicy,RequiresShipping,IsTaxable,Barcode,Weight,IsActive")] Product product)
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
				return RedirectToAction(nameof(Index));
				}

			PopulateDropDownLists(product);
			return View(product);
			}

		// GET: Product/Delete/5
		public async Task<IActionResult> Delete(int? id)
			{
			if (id == null)
				{
				return NotFound();
				}

			var product = await _context.Products
				.Include(p => p.Vendor)
				.Include(p => p.ProductCategory)
				.FirstOrDefaultAsync(m => m.ProductID == id);

			if (product == null)
				{
				return NotFound();
				}

			return View(product);
			}

		// POST: Product/Delete/5 (Archive - Soft Delete)
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
			{
			var product = await _context.Products.FindAsync(id);
			if (product != null)
				{
				// Soft delete - archive instead of removing
				product.IsActive = false;
				_context.Update(product);
				}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
			}

		// ===== HELPER METHODS =====

		private bool ProductExists(int id)
			{
			return _context.Products.Any(e => e.ProductID == id);
			}

		private void PopulateDropDownLists(Product? product = null)
			{
			ViewData["VendorID"] = new SelectList(
				_context.Vendors.Where(v => v.IsActive).OrderBy(v => v.VendorName),
				"VendorID",
				"VendorName",
				product?.VendorID);

			ViewData["ProductCategoryID"] = new SelectList(
				_context.ProductCategories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder),
				"CategoryID",
				"CategoryName",
				product?.ProductCategoryID);
			}

		private string GenerateSlug(string text)
			{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			string slug = text.ToLower().Trim();
			slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
			slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
			slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
			slug = slug.Trim('-');

			return slug;
			}

		private async Task<string> EnsureUniqueHandle(string handle)
			{
			string finalHandle = handle;
			int counter = 1;

			while (await _context.Products.AnyAsync(p => p.Handle == finalHandle))
				{
				finalHandle = $"{handle}-{counter}";
				counter++;
				}

			return finalHandle;
			}

		private async Task<string> GenerateSKU()
			{
			// Simple auto-increment SKU for now
			// Format: PROD-0001, PROD-0002, etc.
			var lastProduct = await _context.Products
				.OrderByDescending(p => p.ProductID)
				.FirstOrDefaultAsync();

			int nextNumber = (lastProduct?.ProductID ?? 0) + 1;
			return $"PROD-{nextNumber:D4}";
			}
		}
	}