using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class AdminBaseController : Controller
    {
        protected readonly PontelloDbContext _context;

        public AdminBaseController(PontelloDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var submittedCount = _context.Orders
                .Count(o => o.Status == "Submitted");

            var actionRequiredCount = _context.Orders
                .Count(o => o.Status == "ActionRequired");

            var pendingApplicationCount = _context.DealerApplications
                .Count(a => a.Status == "Pending");

            // Admin notifications (DealerID == null), newest first, max 20
            var adminNotifications = _context.Notifications
                .Where(n => n.DealerID == null)
                .OrderByDescending(n => n.CreatedDate)
                .Take(20)
                .ToList();

            var unreadCount = adminNotifications.Count(n => !n.IsRead);

            ViewData["SubmittedOrderCount"]       = submittedCount;
            ViewData["ActionRequiredCount"]       = actionRequiredCount;
            ViewData["TotalPendingCount"]         = submittedCount + actionRequiredCount;
            ViewData["PendingApplicationCount"]   = pendingApplicationCount;
            ViewData["AdminNotifications"]        = adminNotifications;
            ViewData["AdminUnreadCount"]          = unreadCount;
        }
    }
}
