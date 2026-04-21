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
        public async Task<IActionResult> Index(
            int year = 0,
            int? dealerId = null)
        {
            if (year == 0) year = DateTime.Now.Year;

            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);

            // Base query scoped to selected year
            var yearOrders = _context.Orders
                .Where(o => o.CreatedDate >= yearStart
                         && o.CreatedDate <= yearEnd
                         && o.Status != "Cancelled");

            // Optional dealer filter
            if (dealerId.HasValue)
                yearOrders = yearOrders.Where(o =>
                    o.DealerID == dealerId.Value);

            var totalOrders = await yearOrders
                .CountAsync();

            var orderValue = await yearOrders
                .SumAsync(o => (decimal?)o.SubtotalAmount)
                ?? 0;

            var avgOrderValue = totalOrders > 0
                ? orderValue / totalOrders : 0;

            var monthlyRaw = await yearOrders
                .GroupBy(o => o.CreatedDate.Month)
                .Select(g => new {
                    Month = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .ToListAsync();

            var monthlyData = Enumerable.Range(1, 12)
                .Select(m => {
                    var found = monthlyRaw
                        .FirstOrDefault(x => x.Month == m);
                    return new {
                        Month = m,
                        MonthName = new DateTime(year, m, 1)
                            .ToString("MMM"),
                        OrderCount = found?.OrderCount ?? 0,
                        Revenue = found?.Revenue ?? 0m
                    };
                }).ToList();

            var maxRevenue = monthlyData.Any()
                ? monthlyData.Max(m => m.Revenue) : 1m;

            var topDealersRaw = await yearOrders
                .GroupBy(o => new {
                    o.DealerID,
                    o.DealerCompanyName
                })
                .Select(g => new {
                    CompanyName = g.Key.DealerCompanyName,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .ToListAsync();

            var topDealers = topDealersRaw
                .OrderByDescending(x => x.Revenue)
                .Take(8)
                .ToList();

            var maxDealerRevenue = topDealers.Any()
                ? topDealers.Max(d => d.Revenue) : 1m;

            var statusRaw = await _context.Orders
                .Where(o => o.CreatedDate >= yearStart
                         && o.CreatedDate <= yearEnd)
                .GroupBy(o => o.Status)
                .Select(g => new {
                    Status = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(o => o.SubtotalAmount)
                })
                .ToListAsync();

            var years = await _context.Orders
                .Select(o => o.CreatedDate.Year)
                .Distinct()
                .ToListAsync();
            years = years.OrderByDescending(y => y)
                .ToList();
            if (!years.Contains(year)) years.Insert(0, year);

            var dealers = await _context.Dealers
                .OrderBy(d => d.CompanyName)
                .Select(d => new {
                    d.DealerID,
                    d.CompanyName
                })
                .ToListAsync();

            ViewData["Year"]             = year;
            ViewData["Years"]            = years;
            ViewData["DealerID"]         = dealerId;
            ViewData["Dealers"]          = dealers;
            ViewData["TotalOrders"]      = totalOrders;
            ViewData["OrderValue"]       = orderValue;
            ViewData["AvgOrderValue"]    = avgOrderValue;
            ViewData["MonthlyData"]      = monthlyData;
            ViewData["MaxRevenue"]       = maxRevenue;
            ViewData["TopDealers"]       = topDealers;
            ViewData["MaxDealerRevenue"] = maxDealerRevenue;
            ViewData["StatusBreakdown"]  = statusRaw;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(
            int year = 0,
            int? dealerId = null)
        {
            if (year == 0) year = DateTime.Now.Year;

            var yearStart = new DateTime(year, 1, 1);
            var yearEnd = new DateTime(year, 12, 31, 23, 59, 59);

            var query = _context.Orders
                .Include(o => o.Dealer)
                .Where(o => o.CreatedDate >= yearStart
                         && o.CreatedDate <= yearEnd);

            if (dealerId.HasValue)
                query = query.Where(o =>
                    o.DealerID == dealerId.Value);

            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine(
                "PO Number,Dealer,Date,Status," +
                "Fulfillment,Billing," +
                "Subtotal,Tax,Shipping,Total");

            foreach (var o in orders)
            {
                csv.AppendLine(
                    $"{o.OrderNumber}," +
                    $"{o.Dealer?.CompanyName}," +
                    $"{o.CreatedDate:yyyy-MM-dd}," +
                    $"{o.Status}," +
                    $"{o.FulfillmentStatus}," +
                    $"{o.BillingStatus}," +
                    $"{o.SubtotalAmount:F2}," +
                    $"{o.TaxAmount:F2}," +
                    $"{o.ShippingCost ?? 0:F2}," +
                    $"{o.TotalAmount:F2}");
            }

            return File(
                System.Text.Encoding.UTF8
                    .GetBytes(csv.ToString()),
                "text/csv",
                $"report-{year}.csv");
        }
    }
}
