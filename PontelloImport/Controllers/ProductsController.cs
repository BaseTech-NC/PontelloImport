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
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
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
        public async Task<IActionResult> Create([Bind("Title,Tags,VendorID,ProductCategoryID")] Product product)
        {
            throw new NotImplementedException("Pending V3 rewrite");
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
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Title,Handle,Tags,VendorID,ProductCategoryID,Status")] Product product)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            throw new NotImplementedException("Pending V3 rewrite");
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

            string handle = title.ToLowerInvariant();

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

            handle = handle.Replace(" ", "-");

            while (handle.Contains("--"))
            {
                handle = handle.Replace("--", "-");
            }

            handle = Regex.Replace(handle, @"[^a-z0-9\-]", "");
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
