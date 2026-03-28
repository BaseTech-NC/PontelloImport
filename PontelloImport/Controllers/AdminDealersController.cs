using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminDealersController : Controller
    {
        private readonly PontelloDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDealersController(PontelloDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /AdminDealers
        public async Task<IActionResult> Index()
        {
            var applications = await _context.DealerApplications
                .Include(a => a.SubmittedAddress)
                .OrderBy(a => a.Status == "Pending" ? 0 : a.Status == "Approved" ? 1 : 2)
                .ThenByDescending(a => a.SubmittedDate)
                .ToListAsync();

            return View(applications);
        }

        // GET: /AdminDealers/ReviewApplication/5
        [HttpGet]
        public async Task<IActionResult> ReviewApplication(int id)
        {
            var application = await _context.DealerApplications
                .Include(a => a.SubmittedAddress)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null) return NotFound();

            return View(application);
        }

        // POST: /AdminDealers/ApproveApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id, string assignedPassword)
        {
            var application = await _context.DealerApplications
                .Include(a => a.SubmittedAddress)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null) return NotFound();

            if (string.IsNullOrWhiteSpace(assignedPassword))
            {
                TempData["Error"] = "A password is required to create the dealer account.";
                return RedirectToAction(nameof(ReviewApplication), new { id });
            }

            // Create ApplicationUser
            var user = new ApplicationUser
            {
                UserName = application.SubmittedEmail,
                Email = application.SubmittedEmail,
                FirstName = application.SubmittedContactName.Split(' ').FirstOrDefault() ?? application.SubmittedContactName,
                LastName = application.SubmittedContactName.Contains(' ')
                    ? string.Join(' ', application.SubmittedContactName.Split(' ').Skip(1))
                    : "",
                UserType = "Dealer",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, assignedPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Could not create account: " + string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(ReviewApplication), new { id });
            }

            await _userManager.AddToRoleAsync(user, "Dealer");

            // Create billing address (copy from application)
            var billing = new Address
            {
                Street   = application.SubmittedAddress?.Street   ?? "",
                City     = application.SubmittedAddress?.City     ?? "",
                Province = application.SubmittedAddress?.Province ?? "",
                PostalCode = application.SubmittedAddress?.PostalCode ?? "",
                Country  = "Canada"
            };
            _context.Addresses.Add(billing);
            await _context.SaveChangesAsync();

            // Default payment terms (Net 30)
            var terms = await _context.PaymentTerms
                .FirstOrDefaultAsync(t => t.TermCode == "NET30")
                ?? await _context.PaymentTerms.FirstOrDefaultAsync();

            // Create Dealer record
            var dealer = new Dealer
            {
                ApplicationUserID = user.Id,
                CompanyName       = application.SubmittedCompanyName,
                ContactPhone      = application.SubmittedContactPhone,
                BillingAddressID  = billing.AddressID,
                PaymentTermsID    = terms?.PaymentTermsID ?? 1,
                BusinessNumber    = application.BusinessNumber,
                IsTaxExempt       = false
            };
            _context.Dealers.Add(dealer);

            // Update application status
            application.Status       = "Approved";
            application.ReviewedBy   = User.Identity?.Name ?? "Admin";
            application.ReviewedDate = DateTime.UtcNow;
            application.ApprovedDealerID = dealer.DealerID; // will be set after save

            await _context.SaveChangesAsync();

            // Set ApprovedDealerID now that dealer.DealerID is assigned
            application.ApprovedDealerID = dealer.DealerID;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Account created for {application.SubmittedCompanyName}. " +
                                  $"Email: {application.SubmittedEmail} / Password: {assignedPassword} — " +
                                  "Send these credentials to the dealer.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminDealers/RejectApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id, string? reason)
        {
            var application = await _context.DealerApplications.FindAsync(id);
            if (application == null) return NotFound();

            application.Status       = "Rejected";
            application.ReviewedBy   = User.Identity?.Name ?? "Admin";
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewNotes  = reason;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(Index));
        }
    }
}
