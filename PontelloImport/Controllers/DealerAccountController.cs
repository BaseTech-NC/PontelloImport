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

            // All addresses belonging to this dealer
            var addresses = await _context.Addresses
                .Where(a => a.DealerID == dealer.DealerID)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();

            // Ensure billing address is in the list
            if (dealer.BillingAddress != null &&
                !addresses.Any(a => a.AddressID == dealer.BillingAddressID))
            {
                addresses.Insert(0, dealer.BillingAddress);
            }

            ViewBag.Addresses = addresses;
            return View(dealer);
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

        // POST: /DealerAccount/AddAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(
            string street, string city,
            string provinceState, string postalZipCode,
            string country, string addressType, bool isDefault)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            var existingCount = await _context.Addresses
                .CountAsync(a => a.DealerID == dealer.DealerID);
            bool isFirst = existingCount == 0;
            bool setAsDefault = isDefault || isFirst;

            if (setAsDefault)
            {
                var others = await _context.Addresses
                    .Where(a => a.DealerID == dealer.DealerID)
                    .ToListAsync();
                foreach (var a in others) a.IsDefault = false;
            }

            var address = new Address
            {
                DealerID = dealer.DealerID,
                Street = street,
                City = city,
                Province = provinceState,
                PostalCode = postalZipCode,
                Country = string.IsNullOrWhiteSpace(country) ? "Canada" : country,
                AddressType = string.IsNullOrWhiteSpace(addressType) ? "Both" : addressType,
                IsDefault = setAsDefault
            };
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            if (setAsDefault)
            {
                dealer.BillingAddressID = address.AddressID;
                dealer.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Address added.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/UpdateAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(
            int addressId,
            string street,
            string city,
            string provinceState,
            string postalZipCode,
            string country,
            string addressType,
            bool isDefault = false)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null) return NotFound();

            address.Street = street;
            address.City = city;
            address.Province = provinceState;
            address.PostalCode = postalZipCode;
            address.Country = string.IsNullOrWhiteSpace(country) ? "Canada" : country;
            address.AddressType = string.IsNullOrWhiteSpace(addressType) ? "Both" : addressType;

            if (isDefault)
            {
                var others = await _context.Addresses
                    .Where(a =>
                        a.DealerID == address.DealerID
                        && a.AddressID != addressId)
                    .ToListAsync();

                foreach (var a in others)
                {
                    bool shouldClear = addressType switch
                    {
                        "Billing"  => a.AddressType == "Billing" || a.AddressType == "Both",
                        "Shipping" => a.AddressType == "Shipping" || a.AddressType == "Both",
                        "Both"     => true,
                        _          => false
                    };
                    if (shouldClear) a.IsDefault = false;
                }
                address.IsDefault = true;
            }

            address.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/SetDefaultAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null) return NotFound();

            // Load all addresses for this dealer
            var allAddresses = await _context.Addresses
                .Where(a => a.DealerID == address.DealerID)
                .ToListAsync();

            // Clear IsDefault on addresses of same type
            foreach (var a in allAddresses)
            {
                if (a.AddressID == addressId) continue;

                bool shouldClear = address.AddressType switch
                {
                    "Billing"  => a.AddressType == "Billing" || a.AddressType == "Both",
                    "Shipping" => a.AddressType == "Shipping" || a.AddressType == "Both",
                    "Both"     => true,
                    _          => false
                };

                if (shouldClear) a.IsDefault = false;
            }

            address.IsDefault = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Default address updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

        // POST: /DealerAccount/DeleteAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var dealer = await GetCurrentDealerAsync();
            if (dealer == null) return NotFound();

            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null || address.DealerID != dealer.DealerID)
                return Forbid();

            var count = await _context.Addresses
                .CountAsync(a => a.DealerID == dealer.DealerID);
            if (count <= 1)
            {
                TempData["Error"] = "Cannot delete your only address.";
                return RedirectToAction(nameof(ManageAccount));
            }

            if (address.IsDefault)
            {
                var next = await _context.Addresses
                    .Where(a => a.DealerID == dealer.DealerID && a.AddressID != addressId)
                    .FirstOrDefaultAsync();
                if (next != null) next.IsDefault = true;
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address deleted.";
            return RedirectToAction(nameof(ManageAccount));
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
