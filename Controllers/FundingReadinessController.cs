using System;
using System.Collections.Generic;
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

[Authorize]
public sealed class FundingReadinessController : FundingControllerBase
{
    // These are preparation indicators, not lender eligibility/credit scoring.
    private static readonly (string Key, string Label, string Help)[] Questions =
    {
        ("registered-with-cipc", "Company registration", "If applicable to your business, do you have official company registration evidence?"),
        ("has-bank-account", "Business bank account", "Do you have a bank account suitable for receiving business funding?"),
        ("has-tax-compliance", "Tax compliance", "Do you have the tax compliance evidence your intended funder asks for?"),
        ("keeps-financial-records", "Financial records", "Do you keep up-to-date income and expense records?"),
        ("has-business-plan", "Business plan", "Do you have a current plan explaining your activities and use of funds?")
    };
    private static readonly string[] Choices = { "Yes", "No", "NotSure" };

    public FundingReadinessController(ApplicationDbContext db, UserManager<ApplicationUser> users)
        : base(db, users) { }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        if (!businessId.HasValue)
        {
            var selected = await GetSingleBusinessIdAsync(userId, ct);
            return selected.HasValue ? RedirectToAction(nameof(Index), new { businessId = selected.Value }) : PickBusiness();
        }
        var access = await GetAccessAsync(businessId.Value, userId, ct);
        if (access is null) return NotFound();

        var latest = await Db.FundingReadinessAssessments.AsNoTracking()
            .Where(x => x.BusinessId == businessId.Value)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new { x.FundingReadinessAssessmentId, x.CreatedAtUtc, x.OverallScore })
            .FirstOrDefaultAsync(ct);
        var saved = latest is null ? new Dictionary<string, string>() :
            await Db.FundingReadinessAnswers.AsNoTracking()
                .Where(x => x.FundingReadinessAssessmentId == latest.FundingReadinessAssessmentId)
                .Select(x => new { x.QuestionKey, x.AnswerValue })
                .ToDictionaryAsync(x => x.QuestionKey, x => x.AnswerValue, ct);
        var model = new FundingReadinessIndexVm {
            BusinessId = businessId.Value, BusinessName = access.Name,
            CanManage = access.CanManage,
            LatestAssessmentId = latest?.FundingReadinessAssessmentId,
            LastCheckedUtc = latest?.CreatedAtUtc,
            LastScore = latest?.OverallScore
        };
        foreach (var q in Questions)
        {
            var answer = saved.TryGetValue(q.Key, out var value) && Choices.Contains(value) ? value : "NotSure";
            model.Questions.Add(new FundingReadinessQuestionVm { Key = q.Key, Label = q.Label, Help = q.Help, Answer = answer });
            if (latest is not null && answer != "Yes")
                model.NextSteps.Add(q.Label + ": confirm the requirement with your intended provider and prepare supporting evidence.");
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(FundingReadinessSubmitVm model, CancellationToken ct)
    {
        if (!CurrentUser(out var userId)) return Challenge();
        var access = await GetAccessAsync(model.BusinessId, userId, ct);
        if (access is null || !access.CanManage) return NotFound();

        string[] answers = { model.Registration, model.BankAccount, model.Tax, model.Bookkeeping, model.BusinessPlan };
        if (answers.Any(x => !Choices.Contains(x)))
            return BadRequest("Select Yes, No or Not sure for every question.");

        var id = Guid.NewGuid();
        var yes = answers.Count(x => x == "Yes");
        var assessment = new FundingReadinessAssessment {
            FundingReadinessAssessmentId = id, BusinessId = model.BusinessId,
            AssessedByUserId = userId, AssessmentVersion = "v1",
            Status = "Completed", OverallScore = yes * 20m, CreatedAtUtc = DateTime.UtcNow
        };
        Db.FundingReadinessAssessments.Add(assessment);
        for (int i = 0; i < Questions.Length; i++)
        {
            Db.FundingReadinessAnswers.Add(new FundingReadinessAnswer {
                FundingReadinessAnswerId = Guid.NewGuid(), FundingReadinessAssessmentId = id,
                QuestionKey = Questions[i].Key, AnswerValue = answers[i],
                EvidenceDocumentId = null, Score = answers[i] == "Yes" ? 1m : 0m
            });
        }
        AddAudit(userId, model.BusinessId, "funding.readiness_assessed", "FundingReadinessAssessment", id);
        // Relational SaveChanges wraps the assessment + answers + audit in one transaction.
        await Db.SaveChangesAsync(ct);
        TempData["SuccessMessage"] = "Your readiness checklist was saved. This is self-reported guidance, not lender approval.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }
}
