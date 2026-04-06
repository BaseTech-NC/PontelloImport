using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class StaffAccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public StaffAccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ManageAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = roles;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePhone(string phone)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            user.PhoneNumber = phone?.Trim();
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Phone number updated.";
            return RedirectToAction(nameof(ManageAccount));
        }

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

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password updated successfully.";
            }
            else
            {
                TempData["Error"] = string.Join(", ",
                    result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(ManageAccount));
        }
    }
}
