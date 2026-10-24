using Microsoft.AspNetCore.Mvc;
using SparkVenture.Data;

namespace SparkVenture.Controllers
{
    /// <summary>
    /// Administration: Manage sectors, reference values, templates, and application configuration.
    /// Implementation notes are in the SparkVenture controller implementation guide.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TODO: Add feature-specific actions, validation, and authorisation.
        // _context is injected so the controller can query the database when implemented.
        // Keep business rules in services rather than placing complex logic here.
        public IActionResult Index()
        {
            return View();
        }
    }
}
