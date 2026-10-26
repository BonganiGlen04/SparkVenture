using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SparkVenture.Infrastructure;

namespace SparkVenture.Controllers;

/// <summary>
/// Friendly business-workspace entry point. Does not duplicate BusinessesController.
/// </summary>
[Authorize]
public sealed class BusinessWorkspaceController : Controller
{
    [HttpGet]
    public IActionResult Index(Guid? businessId = null)
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Admin");

        // The middleware validates explicit or remembered business membership.
        if (HttpContext.Items[ActiveBusinessContextMiddleware.BusinessIdItemKey] is Guid selectedId)
            return RedirectToAction("Details", "Businesses", new { id = selectedId });

        // New users and multi-business users without a selection choose once.
        return RedirectToAction("Index", "Businesses");
    }
}
