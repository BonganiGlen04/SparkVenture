using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SparkVenture.Data;
using SparkVenture.ViewModels;
using System.Security.Claims;

namespace SparkVenture.Controllers
{
    /// <summary>
    /// Dashboard: Overview cards, recent activity, tasks due, and summaries for the signed-in user's authorised businesses.
    /// Implementation notes are in the SparkVenture controller implementation guide.
    /// </summary>
    /// 
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TODO: Add feature-specific actions, validation, and authorisation.
        // _context is injected so the controller can query the database when implemented.
        // Keep business rules in services rather than placing complex logic here.
        public IActionResult Index()
        {
            // Identity's default UserName is the email, so Identity.Name is the fallback.
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "";

            return View(new DashboardViewModel { Email = email });
        }
    }
}
