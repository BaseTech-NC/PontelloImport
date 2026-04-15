using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class NotificationsController : AdminBaseController
    {
        public NotificationsController(PontelloDbContext context) : base(context) { }

        // POST: /Notifications/MarkRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id, string? returnUrl = null)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == null)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "AdminOrders");
        }

        // POST: /Notifications/MarkAllRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead(string? returnUrl = null)
        {
            var unread = await _context.Notifications
                .Where(n => n.DealerID == null && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread)
                n.IsRead = true;
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "AdminOrders");
        }

        // POST: /Notifications/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == null)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "AdminOrders");
        }

        // POST: /Notifications/MarkReadAjax — AJAX endpoint for notification UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReadAjax(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == null)
            {
                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST: /Notifications/DeleteAjax — AJAX endpoint for notification UI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var n = await _context.Notifications.FindAsync(id);
            if (n != null && n.DealerID == null)
            {
                _context.Notifications.Remove(n);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // POST: /Notifications/DeleteAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(string? returnUrl = null)
        {
            var all = await _context.Notifications
                .Where(n => n.DealerID == null)
                .ToListAsync();
            _context.Notifications.RemoveRange(all);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "AdminOrders");
        }

    }
}
