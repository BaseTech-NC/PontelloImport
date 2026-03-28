using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using PontelloImport.ViewModels;

namespace PontelloImport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PontelloDbContext _context;

        public HomeController(ILogger<HomeController> logger, PontelloDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                return RedirectToAction("Index", "AdminOrders");
            if (User.IsInRole("Dealer"))
                return RedirectToAction("Index", "Shop");
            return View();
        }

        // GET: /Home/Apply
        [HttpGet]
        public IActionResult Apply()
        {
            return View(new DealerApplicationViewModel());
        }

        // POST: /Home/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(DealerApplicationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Warn if a pending application already exists for this email
            var existing = await _context.DealerApplications
                .AnyAsync(a => a.SubmittedEmail == model.Email && a.Status == "Pending");
            if (existing)
            {
                ModelState.AddModelError("Email", "An application for this email is already pending review.");
                return View(model);
            }

            // Create address record
            var address = new Address
            {
                Street = model.BusinessAddress,
                City = model.City,
                Province = model.Province,
                PostalCode = model.PostalCode,
                Country = "Canada"
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // Create dealer application
            var application = new DealerApplication
            {
                SubmittedCompanyName = model.CompanyName,
                SubmittedContactName = $"{model.FirstName} {model.LastName}",
                SubmittedContactPhone = model.Phone,
                SubmittedEmail = model.Email,
                SubmittedAddressID = address.AddressID,
                BusinessNumber = model.BusinessNumber,
                Status = "Pending",
                SubmittedDate = DateTime.UtcNow,
                ReviewNotes = model.Notes
            };
            _context.DealerApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Application received. Pontello Imports will review your application and contact you within 2 business days.";
            return RedirectToAction(nameof(Apply));
        }

        // GET: /Home/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
