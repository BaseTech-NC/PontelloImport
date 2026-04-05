using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Models;
using PontelloImport.Data;
using PontelloImport.ViewModels;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminUsersController : AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            PontelloDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminUsersController> logger)
            : base(context)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index()
        {
            var allUsers = await _userManager.Users
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            // Exclude Dealer-role users
            var staffUsers = new List<(ApplicationUser User, IList<string> Roles)>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Dealer"))
                    staffUsers.Add((User: user, Roles: roles));
            }

            return View(staffUsers);
        }

        // GET: /AdminUsers/CreateStaff
        [HttpGet]
        public IActionResult CreateStaff()
        {
            return View(new CreateStaffViewModel());
        }

        // POST: /AdminUsers/CreateStaff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Role != "Admin" && model.Role != "Staff")
            {
                ModelState.AddModelError("Role", "Role must be Admin or Staff.");
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            var tempPassword = GenerateTempPassword(model.FirstName, model.Email);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserType = model.Role,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ",
                    result.Errors.Select(e => e.Description));
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            TempData["Success"] =
                $"Account created for {model.FirstName} {model.LastName}. " +
                $"Email: {model.Email} | Temporary password: {tempPassword}";

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/DeactivateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            TempData["Success"] = $"{user.FirstName} {user.LastName} deactivated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminUsers/ReactivateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.SetLockoutEnabledAsync(user, false);

            TempData["Success"] = $"{user.FirstName} {user.LastName} reactivated.";
            return RedirectToAction(nameof(Index));
        }

        private string GenerateTempPassword(string firstName, string email)
        {
            var clean = new string(
                firstName.Where(char.IsLetter).ToArray());
            var name = clean.Length > 0
                ? char.ToUpper(clean[0]) + clean.Substring(1).ToLower()
                : "Staff";
            var digits = new string(
                email.Where(char.IsDigit).ToArray());
            var suffix = digits.Length >= 4
                ? digits.Substring(digits.Length - 4)
                : new Random().Next(1000, 9999).ToString();
            return $"{name}@{suffix}!";
        }
    }
}
