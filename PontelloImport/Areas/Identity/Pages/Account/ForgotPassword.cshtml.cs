using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PontelloImport.Models;
using PontelloImport.Services;

namespace PontelloImport.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Don't reveal whether the user exists
            if (user == null)
                return RedirectToPage("./ForgotPasswordConfirmation");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new {
                    area = "Identity",
                    code,
                    email = Input.Email
                },
                protocol: "https");

            try
            {
                await _emailService.SendAsync(
                    Input.Email,
                    Input.Email,
                    "Reset your Pontello Imports password",
                    $@"<div style='font-family:sans-serif;max-width:500px;'>
                        <div style='background:#0D3D38;padding:20px;border-radius:8px 8px 0 0;'>
                          <h2 style='color:#02C39A;margin:0;'>Pontello Imports</h2>
                        </div>
                        <div style='background:#f8fafc;padding:20px;border:1px solid #e2e8f0;border-radius:0 0 8px 8px;'>
                          <p style='color:#111827;font-size:15px;'>Password reset requested</p>
                          <p style='color:#6b7280;font-size:13px;'>
                            Click the button below to reset your password.
                            This link expires in 24 hours.
                          </p>
                          <a href='{callbackUrl}'
                             style='display:inline-block;background:#028090;color:#fff;
                                    padding:10px 20px;border-radius:6px;
                                    text-decoration:none;font-size:14px;'>
                            Reset Password
                          </a>
                          <p style='color:#9ca3af;font-size:12px;margin-top:16px;'>
                            If you did not request this, ignore this email.
                          </p>
                        </div>
                      </div>");
            }
            catch
            {
                // Swallow email errors — still redirect to confirmation
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
