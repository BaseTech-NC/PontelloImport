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
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("Staff"))
                return RedirectToAction("Admin", "Dashboard");
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
                .AnyAsync(a => (a.Email == model.Email || a.SubmittedEmail == model.Email) && a.Status == "Pending");
            if (existing)
            {
                ModelState.AddModelError("Email", "An application for this email is already pending review.");
                return View(model);
            }

            var application = new DealerApplication
            {
                // New fields
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Company = model.Company,
                Address = model.Address,
                City = model.City,
                ProvinceState = model.ProvinceState,
                PostalZipCode = model.PostalZipCode,
                CompanyDescription = model.CompanyDescription,
                WebsiteSocialMedia = model.WebsiteSocialMedia,

                // Populate legacy fields for backward compat
                SubmittedCompanyName = model.Company ?? $"{model.FirstName} {model.LastName}",
                SubmittedContactName = $"{model.FirstName} {model.LastName}",
                SubmittedContactPhone = model.Phone,
                SubmittedEmail = model.Email,

                Status = "Pending",
                SubmittedDate = DateTime.UtcNow
            };
            _context.DealerApplications.Add(application);
            await _context.SaveChangesAsync();

            // Admin notification for new application
            _context.Notifications.Add(new PontelloImport.Models.Notification
            {
                DealerID    = null,
                Type        = "ApplicationSubmitted",
                Message     = $"New dealer application from {model.FirstName} {model.LastName}" +
                              (string.IsNullOrWhiteSpace(model.Company) ? "" : $" ({model.Company})"),
                ActionUrl   = $"/AdminDealers/ReviewApplication/{application.ApplicationID}",
                IsRead      = false,
                CreatedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["AppRefId"]    = $"APP-{application.ApplicationID:D5}";
            TempData["AppFirstName"] = model.FirstName;
            TempData["AppEmail"]    = model.Email;
            return RedirectToAction(nameof(ApplyConfirmation));
        }

        // GET: /Home/ApplyConfirmation
        [HttpGet]
        public IActionResult ApplyConfirmation()
        {
            // If accessed directly without going through Apply, send back to Apply
            if (TempData["AppRefId"] == null)
                return RedirectToAction(nameof(Apply));
            return View();
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
