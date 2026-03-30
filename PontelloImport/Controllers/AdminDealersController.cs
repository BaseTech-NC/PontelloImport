using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminDealersController : AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDealersController(PontelloDbContext context, UserManager<ApplicationUser> userManager)
            : base(context)
        {
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

        // ── Dealer Management ────────────────────────────────────────────────

        // GET: /AdminDealers/Dealers
        [HttpGet]
        public async Task<IActionResult> Dealers(string? search, string? status)
        {
            var dealers = await _context.Dealers
                .Include(d => d.BillingAddress)
                .Include(d => d.PaymentTerms)
                .Include(d => d.Orders)
                .OrderBy(d => d.CompanyName)
                .ToListAsync();

            var userIds = dealers
                .Where(d => d.ApplicationUserID != null)
                .Select(d => d.ApplicationUserID!)
                .ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var userDict = users.ToDictionary(u => u.Id);

            bool IsActive(Dealer d)
            {
                if (d.ApplicationUserID == null || !userDict.ContainsKey(d.ApplicationUserID))
                    return true;
                var u = userDict[d.ApplicationUserID];
                return !(u.LockoutEnabled && u.LockoutEnd.HasValue
                         && u.LockoutEnd.Value > DateTimeOffset.UtcNow);
            }

            var allItems = dealers.Select(d => new DealerSummaryViewModel
            {
                Dealer     = d,
                User       = d.ApplicationUserID != null && userDict.ContainsKey(d.ApplicationUserID)
                                 ? userDict[d.ApplicationUserID] : null,
                IsActive   = IsActive(d),
                OrderCount = d.Orders.Count
            }).ToList();

            ViewData["TotalDealers"]     = allItems.Count;
            ViewData["ActiveDealers"]    = allItems.Count(i => i.IsActive);
            ViewData["SuspendedDealers"] = allItems.Count(i => !i.IsActive);
            ViewData["Search"]           = search;
            ViewData["Status"]           = status;

            var filtered = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                filtered = filtered.Where(i =>
                    i.Dealer.CompanyName.ToLower().Contains(q) ||
                    (i.User?.Email?.ToLower().Contains(q) ?? false));
            }

            if (status == "active")
                filtered = filtered.Where(i => i.IsActive);
            else if (status == "suspended")
                filtered = filtered.Where(i => !i.IsActive);

            return View(filtered.ToList());
        }

        // GET: /AdminDealers/DealerProfile/5
        [HttpGet]
        public async Task<IActionResult> DealerProfile(int id)
        {
            var dealer = await _context.Dealers
                .Include(d => d.BillingAddress)
                .Include(d => d.PaymentTerms)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.OrderLines)
                .FirstOrDefaultAsync(d => d.DealerID == id);

            if (dealer == null) return NotFound();

            ApplicationUser? user = null;
            if (dealer.ApplicationUserID != null)
                user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);

            bool isActive = user == null ||
                !(user.LockoutEnabled && user.LockoutEnd.HasValue
                  && user.LockoutEnd.Value > DateTimeOffset.UtcNow);

            var allPaymentTerms = await _context.PaymentTerms
                .OrderBy(t => t.TermCode)
                .ToListAsync();

            var vm = new DealerProfileViewModel
            {
                Dealer          = dealer,
                User            = user,
                IsActive        = isActive,
                RecentOrders    = dealer.Orders.OrderByDescending(o => o.CreatedDate).Take(10).ToList(),
                TotalOrderCount = dealer.Orders.Count,
                TotalOrderValue = dealer.Orders.Sum(o => o.TotalAmount),
                AllPaymentTerms = allPaymentTerms
            };

            return View(vm);
        }

        // POST: /AdminDealers/SuspendDealer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendDealer(int id)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            if (dealer.ApplicationUserID != null)
            {
                var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);
                if (user != null)
                {
                    await _userManager.SetLockoutEnabledAsync(user, true);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                }
            }

            TempData["Success"] = $"{dealer.CompanyName} account suspended.";
            return RedirectToAction(nameof(Dealers));
        }

        // POST: /AdminDealers/ReinstateDealer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReinstateDealer(int id)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            if (dealer.ApplicationUserID != null)
            {
                var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.SetLockoutEnabledAsync(user, false);
                }
            }

            TempData["Success"] = $"{dealer.CompanyName} account reinstated.";
            return RedirectToAction(nameof(Dealers));
        }

        // POST: /AdminDealers/UpdateDealerTerms/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDealerTerms(int id, int paymentTermsId, bool isTaxExempt)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            dealer.PaymentTermsID = paymentTermsId;
            dealer.IsTaxExempt    = isTaxExempt;
            dealer.ModifiedDate   = DateTime.UtcNow;
            dealer.ModifiedBy     = User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Dealer terms updated.";
            return RedirectToAction(nameof(DealerProfile), new { id });
        }
    }
}
