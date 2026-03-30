using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PontelloImport.Data;

namespace PontelloImport.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
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

            ViewData["SubmittedOrderCount"]  = submittedCount;
            ViewData["ActionRequiredCount"]  = actionRequiredCount;
            ViewData["TotalPendingCount"]    = submittedCount + actionRequiredCount;
        }
    }
}
