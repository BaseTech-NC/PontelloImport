using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PontelloImport.Data;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Staff")]
    public class ReportsController : AdminBaseController
    {
        public ReportsController(PontelloDbContext context) : base(context) { }

        [HttpGet]
        public async Task<IActionResult> Index(int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;

            // Sales by month for selected year
            var salesByMonth = await _context.Orders
                .Where(o => o.CreatedDate.Year == year && o.Status != "Cancelled")
                .GroupBy(o => o.CreatedDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Top dealers by revenue (all time) — pull to memory first, then sort client-side
            // (SQLite does not support ORDER BY on decimal expressions)
            var topDealers = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .GroupBy(o => new { o.DealerID, o.DealerCompanyName })
                .Select(g => new
                {
                    g.Key.DealerID,
                    CompanyName = g.Key.DealerCompanyName,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .ToListAsync();

            topDealers = topDealers
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            // Order status breakdown
            var statusBreakdown = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .OrderByDescending(g => g.Count)
                .ToListAsync();

            // Available years
            var years = await _context.Orders
                .Select(o => o.CreatedDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
            if (!years.Contains(year)) years.Add(year);
            years = years.OrderByDescending(y => y).ToList();

            // Monthly totals for bar display (all 12 months)
            var monthData = Enumerable.Range(1, 12).Select(m =>
            {
                var found = salesByMonth.FirstOrDefault(x => x.Month == m);
                return new
                {
                    Month = m,
                    MonthName = new DateTime(year, m, 1).ToString("MMM"),
                    OrderCount = found?.OrderCount ?? 0,
                    Revenue = found?.Revenue ?? 0m
                };
            }).ToList();

            var maxRevenue = monthData.Max(m => m.Revenue);
            if (maxRevenue == 0) maxRevenue = 1; // avoid division by zero

            var totalRevenue = salesByMonth.Sum(m => m.Revenue);
            var totalOrders = salesByMonth.Sum(m => m.OrderCount);
            var avgOrder = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            ViewData["Year"] = year;
            ViewData["Years"] = years;
            ViewData["SalesByMonth"] = monthData;
            ViewData["MaxRevenue"] = maxRevenue;
            ViewData["TopDealers"] = topDealers;
            ViewData["StatusBreakdown"] = statusBreakdown;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["AvgOrder"] = avgOrder;

            return View();
        }
    }
}
