using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SparkVenture.Controllers;

// Single, role-aware entry point after sign-in.
// Keep role membership under ASP.NET Core Identity; never trust a querystring role.
[Authorize]
public sealed class WorkspaceController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Admin");

        return RedirectToAction("Index", "Dashboard");
    }
}
