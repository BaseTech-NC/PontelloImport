using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class AuditHistoryController : AdminBaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditHistoryController(
            PontelloDbContext context,
            UserManager<ApplicationUser> userManager)
            : base(context)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            string? actionType = null,
            string? userId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 25)
        {
            bool isStaff = User.IsInRole("Staff") &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("SuperAdmin");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.OrderHistories
                .Include(h => h.Order)
                    .ThenInclude(o => o!.Dealer)
                .AsQueryable();

            if (isStaff)
                query = query.Where(h => h.ChangedBy == currentUserId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(h =>
                    (h.Order != null && h.Order.OrderNumber.Contains(search))
                    || h.ChangeDescription.Contains(search)
                    || (h.Order != null && h.Order.Dealer != null &&
                        h.Order.Dealer.CompanyName.Contains(search)));

            if (!string.IsNullOrWhiteSpace(actionType))
                query = query.Where(h => h.ChangeType == actionType);

            if (!string.IsNullOrWhiteSpace(userId))
                query = query.Where(h => h.ChangedBy == userId);

            if (dateFrom.HasValue)
                query = query.Where(h => h.ChangedDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(h => h.ChangedDate <= dateTo.Value.AddDays(1));

            var total = await query.CountAsync();
            var entries = await query
                .OrderByDescending(h => h.ChangedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var allStaff = adminUsers
                .Concat(superAdmins)
                .DistinctBy(u => u.Id)
                .ToList();

            ViewData["Search"]     = search;
            ViewData["ActionType"] = actionType;
            ViewData["UserId"]     = userId;
            ViewData["DateFrom"]   = dateFrom;
            ViewData["DateTo"]     = dateTo;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"]   = pageSize;
            ViewData["TotalCount"] = total;
            ViewData["TotalPages"] = (int)Math.Ceiling((double)total / pageSize);
            ViewData["StaffUsers"] = allStaff;
            ViewData["ActionTypes"] = new[]
            {
                "Submitted", "Confirmed", "Shipped", "Invoiced", "Cancelled",
                "FlaggedIssue", "EditInitiated", "AdminModified", "Resolved",
                "TrackingAdded"
            };

            return View(entries);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(
            string? search = null,
            string? actionType = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.OrderHistories
                .Include(h => h.Order)
                    .ThenInclude(o => o!.Dealer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(h =>
                    (h.Order != null && h.Order.OrderNumber.Contains(search))
                    || h.ChangeDescription.Contains(search));

            if (!string.IsNullOrWhiteSpace(actionType))
                query = query.Where(h => h.ChangeType == actionType);

            if (dateFrom.HasValue)
                query = query.Where(h => h.ChangedDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(h => h.ChangedDate <= dateTo.Value.AddDays(1));

            var data = await query
                .OrderByDescending(h => h.ChangedDate)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Date,Action,Order,Dealer,Description,Changed By");
            foreach (var h in data)
            {
                var desc = h.ChangeDescription?.Replace("\"", "\"\"") ?? "";
                csv.AppendLine(
                    $"{h.ChangedDate:yyyy-MM-dd HH:mm}," +
                    $"{h.ChangeType}," +
                    $"{h.Order?.OrderNumber}," +
                    $"{h.Order?.Dealer?.CompanyName}," +
                    $"\"{desc}\"," +
                    $"{h.ChangedBy}");
            }

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                $"audit-history-{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
