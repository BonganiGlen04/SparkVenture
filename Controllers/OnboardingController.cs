using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;

namespace SparkVenture.Controllers
{

    [Authorize]
    public class OnboardingController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OnboardingController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User)!);

            var businesses = await _db.BusinessMembers
                .Where(m =>
                    m.UserId == userId &&
                    m.MembershipStatus == "Active")
                .Select(m => m.BusinessId)
                .Distinct()
                .ToListAsync();

            if (businesses.Count < 1)
            {
                return RedirectToAction(
                    "CreateBusiness", "Businesses");
            }

            if (businesses.Count >= 1)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { businessId = businesses[0] });
            }

            return RedirectToAction(
                "Index", "Businesses");
        }
    }

}
