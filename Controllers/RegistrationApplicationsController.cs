using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkVenture.Data;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

/// <summary>
/// Phase B, Step 1: list, start, and view draft registration-preparation applications.
/// This does NOT submit an application to CIPC.
/// </summary>
[Authorize]
public sealed class RegistrationApplicationsController : Controller
{
    private const string CompanyWorkflow = "CompanyRegistration";
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public RegistrationApplicationsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(_users.GetUserId(User), out userId);

    // For initial MVP: only active owners/managers can prepare registrations.
    // Reviewers get separate authorisation in Phase B4.
    private Task<bool> CanManageAsync(Guid businessId, Guid userId, CancellationToken ct) =>
        _db.BusinessMembers.AsNoTracking().AnyAsync(m =>
            m.BusinessId == businessId &&
            m.UserId == userId &&
            m.MembershipStatus == "Active" &&
            (m.BusinessRole == "Owner" || m.BusinessRole == "Manager"), ct);

    // GET: /RegistrationApplications?businessId=<guid>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();

        // Sidebar links may not carry business context.
        if (businessId is null)
        {
            var allowedIds = await _db.BusinessMembers.AsNoTracking()
                .Where(m => m.UserId == userId && m.MembershipStatus == "Active" &&
                    (m.BusinessRole == "Owner" || m.BusinessRole == "Manager"))
                .Select(m => m.BusinessId).Distinct().Take(2)
                .ToListAsync(cancellationToken);

            if (allowedIds.Count != 1)
                return RedirectToAction("Index", "Businesses");

            return RedirectToAction(nameof(Index), new { businessId = allowedIds[0] });
        }

        if (businessId.Value == Guid.Empty ||
            !await CanManageAsync(businessId.Value, userId, cancellationToken))
            return NotFound();

        var name = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == businessId.Value)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null) return NotFound();

        var applications = await _db.RegistrationApplications.AsNoTracking()
            .Where(a => a.BusinessId == businessId.Value && a.WorkflowType == CompanyWorkflow)
            .OrderByDescending(a => a.UpdatedAtUtc)
            .Take(50)
            .Select(a => new RegistrationApplicationListItemViewModel
            {
                RegistrationApplicationId = a.RegistrationApplicationId,
                WorkflowType = a.WorkflowType,
                Status = a.Status,
                CurrentStepKey = a.CurrentStepKey,
                CreatedAtUtc = a.CreatedAtUtc,
                UpdatedAtUtc = a.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return View(new RegistrationApplicationsIndexViewModel
        {
            BusinessId = businessId.Value,
            BusinessName = name,
            Applications = applications
        });
    }

    // POST: /RegistrationApplications/Start
    // Creates the draft + snapshots of active registration task templates.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(Guid businessId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        if (businessId == Guid.Empty ||
            !await CanManageAsync(businessId, userId, cancellationToken))
            return NotFound();

        // Resume an existing non-final application rather than creating another draft.
        var existing = await _db.RegistrationApplications.AsNoTracking()
            .Where(a => a.BusinessId == businessId && a.WorkflowType == CompanyWorkflow &&
                (a.Status == "Draft" || a.Status == "Returned" ||
                 a.Status == "Submitted" || a.Status == "InReview"))
            .OrderByDescending(a => a.UpdatedAtUtc)
            .Select(a => (Guid?)a.RegistrationApplicationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing.HasValue)
            return RedirectToAction(nameof(Details), new { id = existing.Value });

        var templates = await _db.RegistrationTaskTemplates.AsNoTracking()
            .Where(t => t.WorkflowType == CompanyWorkflow && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Select(t => new
            {
                t.RegistrationTaskTemplateId,
                t.StepKey,
                t.Title,
                t.Instructions
            })
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            TempData["ErrorMessage"] =
                "Company-registration steps are not configured yet. Seed RegistrationTaskTemplates first.";
            return RedirectToAction(nameof(Index), new { businessId });
        }

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _db.RegistrationApplications.Add(new RegistrationApplication
        {
            RegistrationApplicationId = id,
            BusinessId = businessId,
            CreatedByUserId = userId,
            WorkflowType = CompanyWorkflow,
            Status = "Draft",
            CurrentStepKey = templates[0].StepKey,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        for (var index = 0; index < templates.Count; index++)
        {
            var step = templates[index];
            _db.RegistrationApplicationTasks.Add(new RegistrationApplicationTask
            {
                RegistrationApplicationTaskId = Guid.NewGuid(),
                RegistrationApplicationId = id,
                RegistrationTaskTemplateId = step.RegistrationTaskTemplateId,
                AssignedToUserId = userId,
                TaskKey = step.StepKey,
                TitleSnapshot = step.Title,
                InstructionsSnapshot = step.Instructions,
                Status = index == 0 ? "InProgress" : "Pending"
            });
        }

        _db.RegistrationReviewEvents.Add(new RegistrationReviewEvent
        {
            RegistrationReviewEventId = Guid.NewGuid(),
            RegistrationApplicationId = id,
            ActorUserId = userId,
            FromStatus = null,
            ToStatus = "Draft",
            EventType = "Created",
            Comment = "Registration preparation started.",
            OccurredAtUtc = now
        });

        // EF Core's relational SaveChanges uses a transaction for these related inserts.
        await _db.SaveChangesAsync(cancellationToken);

        TempData["SuccessMessage"] = "Registration draft created. You can return to it later.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /RegistrationApplications/Details/<guid>
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Challenge();
        if (id == Guid.Empty) return NotFound();

        var application = await _db.RegistrationApplications.AsNoTracking()
            .Where(a => a.RegistrationApplicationId == id && a.WorkflowType == CompanyWorkflow)
            .Select(a => new
            {
                a.RegistrationApplicationId,
                a.BusinessId,
                a.WorkflowType,
                a.Status,
                a.CurrentStepKey,
                a.UpdatedAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null ||
            !await CanManageAsync(application.BusinessId, userId, cancellationToken))
            return NotFound();

        var name = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == application.BusinessId)
            .Select(b => b.TradingName)
            .SingleOrDefaultAsync(cancellationToken);
        if (name is null) return NotFound();

        var steps = await (
            from task in _db.RegistrationApplicationTasks.AsNoTracking()
            join template in _db.RegistrationTaskTemplates.AsNoTracking()
                on task.RegistrationTaskTemplateId equals template.RegistrationTaskTemplateId
            where task.RegistrationApplicationId == id
            orderby template.SortOrder
            select new RegistrationStepViewModel
            {
                StepKey = task.TaskKey,
                Title = task.TitleSnapshot,
                Instructions = task.InstructionsSnapshot,
                Status = task.Status
            }).ToListAsync(cancellationToken);

        return View(new RegistrationApplicationDetailsViewModel
        {
            RegistrationApplicationId = application.RegistrationApplicationId,
            BusinessId = application.BusinessId,
            BusinessName = name,
            WorkflowType = application.WorkflowType,
            Status = application.Status,
            CurrentStepKey = application.CurrentStepKey,
            UpdatedAtUtc = application.UpdatedAtUtc,
            Steps = steps
        });
    }
}
