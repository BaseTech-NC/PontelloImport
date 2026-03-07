using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PontelloImport.Models;

namespace PontelloImport.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EnterDealerPortal()
        {
            HttpContext.Session.SetString("DemoRole", "Dealer");
            return Redirect("/Shop");
        }

        public IActionResult EnterAdminPortal()
        {
            HttpContext.Session.SetString("DemoRole", "Admin");
            return Redirect("/AdminOrders");
        }

        public IActionResult ExitPortal()
        {
            HttpContext.Session.Remove("DemoRole");
            return Redirect("/");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
