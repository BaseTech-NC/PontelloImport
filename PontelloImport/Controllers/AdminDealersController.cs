using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using PontelloImport.Services;
using PontelloImport.ViewModels;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminDealersController : AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminDealersController> _logger;

        public AdminDealersController(
            PontelloDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<AdminDealersController> logger)
            : base(context)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /AdminDealers
        public async Task<IActionResult> Index(
            string? tab    = "pending",
            string? search = null,
            string? region = null,
            string? from   = null,
            string? to     = null,
            int     page   = 1)
        {
            const int PageSize = 20;

            var all = await _context.DealerApplications
                .Include(a => a.SubmittedAddress)
                .OrderByDescending(a => a.SubmittedDate)
                .ToListAsync();

            // Tab counts (unfiltered by search/region/date)
            ViewData["CountPending"]  = all.Count(a => a.Status == "Pending");
            ViewData["CountApproved"] = all.Count(a => a.Status == "Approved");
            ViewData["CountRejected"] = all.Count(a => a.Status == "Rejected");
            ViewData["CountAll"]      = all.Count;

            // Apply tab filter
            var filtered = all.AsEnumerable();
            if (tab == "pending")  filtered = filtered.Where(a => a.Status == "Pending");
            else if (tab == "approved") filtered = filtered.Where(a => a.Status == "Approved");
            else if (tab == "rejected") filtered = filtered.Where(a => a.Status == "Rejected");

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                filtered = filtered.Where(a =>
                    (a.FirstName + " " + a.LastName).ToLower().Contains(q) ||
                    (a.Company?.ToLower().Contains(q) ?? false) ||
                    (a.Email?.ToLower().Contains(q) ?? false) ||
                    (a.SubmittedCompanyName?.ToLower().Contains(q) ?? false) ||
                    (a.City?.ToLower().Contains(q) ?? false) ||
                    (a.ProvinceState?.ToLower().Contains(q) ?? false) ||
                    (a.SubmittedAddress?.City?.ToLower().Contains(q) ?? false) ||
                    (a.SubmittedAddress?.Province?.ToLower().Contains(q) ?? false));
            }

            // Date range
            if (DateTime.TryParse(from, out var fromDate))
                filtered = filtered.Where(a => a.SubmittedDate >= fromDate.ToUniversalTime());
            if (DateTime.TryParse(to, out var toDate))
                filtered = filtered.Where(a => a.SubmittedDate <= toDate.AddDays(1).ToUniversalTime());

            var list   = filtered.ToList();
            var total  = list.Count;
            var paged  = list.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            ViewData["Tab"]        = tab;
            ViewData["Search"]     = search;
            ViewData["Region"]     = region;
            ViewData["From"]       = from;
            ViewData["To"]         = to;
            ViewData["Page"]       = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"] = total;

            return View(paged);
        }

        // GET: /AdminDealers/ReviewApplication/5
        [HttpGet]
        public async Task<IActionResult> ReviewApplication(int id)
        {
            var application = await _context.DealerApplications
                .Include(a => a.SubmittedAddress)
                .FirstOrDefaultAsync(a => a.ApplicationID == id);

            if (application == null) return NotFound();

            var paymentTerms = await _context.PaymentTerms
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
            ViewBag.PaymentTerms = paymentTerms;

            return View(application);
        }

        private string GenerateTempPassword(string firstName, string phone)
        {
            var clean = new string(
                firstName.Where(char.IsLetter).ToArray());
            var name = clean.Length > 0
                ? char.ToUpper(clean[0]) + clean.Substring(1).ToLower()
                : "Dealer";
            var digits = new string(
                phone.Where(char.IsDigit).ToArray());
            var last4 = digits.Length >= 4
                ? digits.Substring(digits.Length - 4)
                : new Random().Next(1000, 9999).ToString();
            return $"{name}@{last4}!";
        }

        // POST: /AdminDealers/ApproveApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveApplication(int id,
            int paymentTermsId = 1, bool isTaxExempt = false)
        {
            var app = await _context.DealerApplications
                .FindAsync(id);
            if (app == null) return NotFound();

            var existing = await _userManager
                .FindByEmailAsync(app.Email);
            if (existing != null)
            {
                TempData["Error"] =
                    "An account with this email already exists.";
                return RedirectToAction(
                    nameof(ReviewApplication), new { id });
            }

            var tempPassword = GenerateTempPassword(
                app.FirstName, app.Phone ?? "0000");

            var user = new ApplicationUser
            {
                UserName = app.Email,
                Email = app.Email,
                FirstName = app.FirstName,
                LastName = app.LastName,
                UserType = "Dealer",
                EmailConfirmed = true
            };

            var result = await _userManager
                .CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ",
                    result.Errors.Select(e => e.Description));
                return RedirectToAction(
                    nameof(ReviewApplication), new { id });
            }

            await _userManager.AddToRoleAsync(user, "Dealer");

            // Create billing address from application fields
            var billing = new Address
            {
                Street = app.Address ?? app.SubmittedAddress?.Street ?? "",
                City = app.City ?? app.SubmittedAddress?.City ?? "",
                Province = app.ProvinceState ?? app.SubmittedAddress?.Province ?? "",
                PostalCode = app.PostalZipCode ?? app.SubmittedAddress?.PostalCode ?? "",
                Country = "Canada"
            };
            _context.Addresses.Add(billing);
            await _context.SaveChangesAsync();

            // Use admin-selected payment terms, falling back to default if not found
            var selectedTerms = await _context.PaymentTerms
                .FirstOrDefaultAsync(t => t.PaymentTermsID == paymentTermsId);
            if (selectedTerms == null)
            {
                selectedTerms = await _context.PaymentTerms
                    .FirstOrDefaultAsync(t => t.TermCode == "NET30")
                    ?? await _context.PaymentTerms.FirstOrDefaultAsync();
            }

            var dealer = new Dealer
            {
                ApplicationUserID = user.Id,
                CompanyName = app.Company ?? $"{app.FirstName} {app.LastName}",
                ContactPhone = app.Phone ?? app.SubmittedContactPhone ?? "",
                BillingAddressID = billing.AddressID,
                PaymentTermsID = selectedTerms?.PaymentTermsID ?? paymentTermsId,
                IsTaxExempt = isTaxExempt
            };
            _context.Dealers.Add(dealer);

            app.Status = "Approved";
            app.ReviewedDate = DateTime.UtcNow;
            app.ReviewedBy = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // Set ApprovedDealerID now that dealer.DealerID is assigned
            app.ApprovedDealerID = dealer.DealerID;
            await _context.SaveChangesAsync();

            bool emailSent = false;
            try
            {
                await _emailService.SendDealerApprovedAsync(
                    app.Email, app.FirstName, tempPassword);
                emailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send approval email to {Email}", app.Email);
            }

            // Also notify admin Gmail about the new dealer
            try
            {
                await _emailService.SendAsync(
                    "noreply.pontelloimports@gmail.com",
                    "Pontello Admin",
                    $"New dealer approved — {app.FirstName} {app.LastName}",
                    $"<p>Dealer account created.</p>" +
                    $"<p>Email: {app.Email}<br>" +
                    $"Temp password: {tempPassword}</p>" +
                    $"<p>Company: {app.Company ?? "—"}</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin approval notification");
            }

            TempData["ApprovedName"]     = $"{app.FirstName} {app.LastName}";
            TempData["ApprovedCompany"]  = app.Company ?? "";
            TempData["ApprovedEmail"]    = app.Email;
            TempData["ApprovedPassword"] = tempPassword;
            TempData["ApprovedRefId"]    = $"APP-{app.ApplicationID:D5}";
            TempData["EmailSent"]        = emailSent;

            return RedirectToAction(nameof(ApprovalConfirmation), new { id });
        }

        // GET: /AdminDealers/ApprovalConfirmation/5
        [HttpGet]
        public IActionResult ApprovalConfirmation(int id)
        {
            if (TempData["ApprovedEmail"] == null)
                return RedirectToAction(nameof(Index));
            return View();
        }

        // POST: /AdminDealers/RejectApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id, string? reason)
        {
            var application = await _context.DealerApplications.FindAsync(id);
            if (application == null) return NotFound();

            application.Status = "Rejected";
            application.ReviewedBy = User.Identity?.Name ?? "Admin";
            application.ReviewedDate = DateTime.UtcNow;
            application.ReviewNotes = reason;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(Index));
        }


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
                Dealer = d,
                User = d.ApplicationUserID != null && userDict.ContainsKey(d.ApplicationUserID)
                           ? userDict[d.ApplicationUserID] : null,
                IsActive = IsActive(d),
                OrderCount = d.Orders.Count
            }).ToList();

            ViewData["TotalDealers"] = allItems.Count;
            ViewData["ActiveDealers"] = allItems.Count(i => i.IsActive);
            ViewData["SuspendedDealers"] = allItems.Count(i => !i.IsActive);
            ViewData["Search"] = search;
            ViewData["Status"] = status;

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
                Dealer = dealer,
                User = user,
                IsActive = isActive,
                RecentOrders = dealer.Orders.OrderByDescending(o => o.CreatedDate).Take(10).ToList(),
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

        // POST: /AdminDealers/ChangeDealerEmail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeDealerEmail(int id, string newEmail)
        {
            var dealer = await _context.Dealers
                .FirstOrDefaultAsync(d => d.DealerID == id);
            if (dealer == null) return NotFound();

            if (dealer.ApplicationUserID == null)
            {
                TempData["Error"] = "Dealer has no associated user account.";
                return RedirectToAction(nameof(DealerProfile), new { id });
            }

            var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains('@'))
            {
                TempData["Error"] = "Invalid email address.";
                return RedirectToAction(nameof(DealerProfile), new { id });
            }

            var existing = await _userManager.FindByEmailAsync(newEmail);
            if (existing != null && existing.Id != user.Id)
            {
                TempData["Error"] = "That email is already registered.";
                return RedirectToAction(nameof(DealerProfile), new { id });
            }

            var oldEmail = user.Email ?? "";

            user.Email = newEmail;
            user.UserName = newEmail;
            user.NormalizedEmail = newEmail.ToUpperInvariant();
            user.NormalizedUserName = newEmail.ToUpperInvariant();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(DealerProfile), new { id });
            }

            // Notify old email
            try
            {
                await _emailService.SendAsync(oldEmail, dealer.CompanyName,
                    "Your Pontello Imports login email changed",
                    $"<p>Your login email has been updated by Pontello Imports.</p>" +
                    $"<p>New login email: {newEmail}</p>" +
                    $"<p>If you did not request this, contact 647-964-6833 immediately.</p>");

                await _emailService.SendAsync(newEmail, dealer.CompanyName,
                    "Welcome — your Pontello Imports login email is confirmed",
                    $"<p>Your Pontello Imports dealer account is now linked to this email address.</p>" +
                    $"<p>Use this email to sign in: {newEmail}</p>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email change notify failed for dealer {Id}", id);
            }

            TempData["Success"] = $"Email updated from {oldEmail} to {newEmail}. Both addresses notified.";
            return RedirectToAction(nameof(DealerProfile), new { id });
        }

        // POST: /AdminDealers/UpdateDealerTerms/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDealerTerms(int id, int paymentTermsId, bool isTaxExempt)
        {
            var dealer = await _context.Dealers.FindAsync(id);
            if (dealer == null) return NotFound();

            dealer.PaymentTermsID = paymentTermsId;
            dealer.IsTaxExempt = isTaxExempt;
            dealer.ModifiedDate = DateTime.UtcNow;
            dealer.ModifiedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Dealer terms updated.";
            return RedirectToAction(nameof(DealerProfile), new { id });
        }
    }
}
