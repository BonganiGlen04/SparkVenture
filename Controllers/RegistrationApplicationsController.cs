using Microsoft.AspNetCore.Mvc;
using SparkVenture.Data;

namespace SparkVenture.Controllers
{
    /// <summary>
    /// Registration applications: Create and edit drafts, manage application details, submit applications, and show progress.
    /// Implementation notes are in the SparkVenture controller implementation guide.
    /// </summary>
    public class RegistrationApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistrationApplicationsController(ApplicationDbContext context)
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
