using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;
using System.Security.Claims;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class DealerAccountController : Controller
    {
        private readonly PontelloDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DealerAccountController(
            PontelloDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<Dealer?> GetCurrentDealerAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;
            return await _context.Dealers
                .Include(d => d.BillingAddress)
                .Include(d => d.ShippingAddress)
                .Include(d => d.PaymentTerms)
                .FirstOrDefaultAsync(d => d.ApplicationUserID == userId);
        }

        // GET: /DealerAccount/ManageAccount
        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null)
            {
                TempData["Error"] = "Dealer account not found.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(dealer.ApplicationUserID!);
            ViewBag.User = user;
            ViewBag.Dealer = dealer;

            // All addresses belonging to this dealer
            var addresses = await _context.Addresses
                .Where(a => a.DealerID == dealer.DealerID)
                .OrderByDescending(a => a.IsPrimary)
                .ToListAsync();

            // Ensure billing address is in the list
            if (dealer.BillingAddress != null &&
                !addresses.Any(a => a.AddressID == dealer.BillingAddressID))
            {
                addresses.Insert(0, dealer.BillingAddress);
            }

            ViewBag.Addresses = addresses;
            return View();
        }

        // POST: /DealerAccount/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string phone)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            dealer.ContactPhone = phone?.Trim() ?? dealer.ContactPhone;
            dealer.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/UpdateAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(
            int addressId, string street, string city,
            string province, string postalCode, string country,
            bool setAsPrimary)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null ||
                (address.DealerID != dealer.DealerID &&
                 address.AddressID != dealer.BillingAddressID &&
                 address.AddressID != dealer.ShippingAddressID))
                return Forbid();

            address.Street = street;
            address.City = city;
            address.Province = province;
            address.PostalCode = postalCode;
            address.Country = country;
            address.ModifiedDate = DateTime.UtcNow;

            if (setAsPrimary)
                await SetPrimaryInternalAsync(dealer, address.AddressID);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Address updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/AddAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(
            string street, string city, string province,
            string postalCode, string country, bool setAsPrimary)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            var existingCount = await _context.Addresses
                .CountAsync(a => a.DealerID == dealer.DealerID);
            var isFirst = existingCount == 0 && dealer.ShippingAddressID == null;

            var address = new Address
            {
                Street = street,
                City = city,
                Province = province,
                PostalCode = postalCode,
                Country = string.IsNullOrWhiteSpace(country) ? "Canada" : country,
                DealerID = dealer.DealerID,
                IsPrimary = isFirst || setAsPrimary
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            // If first address or set as primary, make it the billing address
            if (isFirst || setAsPrimary)
            {
                // Reset other addresses
                var others = await _context.Addresses
                    .Where(a => a.DealerID == dealer.DealerID && a.AddressID != address.AddressID)
                    .ToListAsync();
                foreach (var a in others) a.IsPrimary = false;
                dealer.BillingAddressID = address.AddressID;
                dealer.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Secondary address — set as shipping
                dealer.ShippingAddressID = address.AddressID;
                dealer.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Address added.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/SetPrimaryAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryAddress(int addressId)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            await SetPrimaryInternalAsync(dealer, addressId);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Primary address updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

        private async Task SetPrimaryInternalAsync(Dealer dealer, int primaryAddressId)
        {
            // Reset all dealer-linked addresses
            var all = await _context.Addresses
                .Where(a => a.DealerID == dealer.DealerID)
                .ToListAsync();
            foreach (var a in all) a.IsPrimary = false;

            // Also handle billing address if it's not in the DealerID collection
            if (dealer.BillingAddress != null)
                dealer.BillingAddress.IsPrimary = false;

            var primary = await _context.Addresses.FindAsync(primaryAddressId);
            if (primary != null)
            {
                primary.IsPrimary = true;
                dealer.BillingAddressID = primaryAddressId;

                // If this was the shipping address, clear that pointer
                if (dealer.ShippingAddressID == primaryAddressId)
                    dealer.ShippingAddressID = null;
            }

            dealer.ModifiedDate = DateTime.UtcNow;
        }

        // POST: /DealerAccount/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction(nameof(ManageAccount));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(" ",
                    result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(ManageAccount));
        }
    }
}
