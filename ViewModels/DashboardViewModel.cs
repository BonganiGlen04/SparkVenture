using System;
using System.Collections.Generic;

namespace SparkVenture.ViewModels;

/// <summary>
/// Data for one business workspace. All values are scoped to BusinessId by the controller.
/// This is onboarding progress, NOT a funding-readiness assessment.
/// </summary>
public sealed class DashboardViewModel
{
    public Guid BusinessId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessRole { get; set; } = string.Empty;
    public string Sector { get; set; } = "Not specified";
    public string Province { get; set; } = "Not specified";
    public string BusinessStage { get; set; } = "Not specified";

    public decimal IncomeThisMonth { get; set; }
    public decimal ExpensesThisMonth { get; set; }
    public decimal NetCashThisMonth => IncomeThisMonth - ExpensesThisMonth;
    public decimal OutstandingInvoices { get; set; }
    public int OpenInvoiceCount { get; set; }
    public int SavedOpportunitiesCount { get; set; }
    public int UnreadNotificationsCount { get; set; }

    /// <summary>Internal preparation/review status; not an official CIPC filing status.</summary>
    public string? RegistrationPreparationStatus { get; set; }
    public bool HasCipcRegistrationNumber { get; set; }

    public List<DashboardTaskViewModel> GettingStartedTasks { get; set; } = new();
    public int CompletedTaskCount => GettingStartedTasks.FindAll(t => t.IsComplete).Count;
    public int TaskCount => GettingStartedTasks.Count;
    public int GettingStartedPercent => TaskCount == 0
        ? 0
        : (int)Math.Round(100.0 * CompletedTaskCount / TaskCount);

    public List<DashboardMonthViewModel> CashflowMonths { get; set; } = new();
    public List<DashboardActivityViewModel> RecentActivity { get; set; } = new();
}

public sealed class DashboardTaskViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public bool UsesBusinessIdAsId { get; set; }
}

public sealed class DashboardMonthViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public int IncomeBarPercent { get; set; }
    public int ExpensesBarPercent { get; set; }
}

public sealed class DashboardActivityViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string Icon { get; set; } = "•";
    public string Kind { get; set; } = "notification"; // notification, income or expense
    public string IconBgClass { get; set; } = "bg-blue-100";
    public string IconTextClass { get; set; } = "text-blue-700";
    public string Controller { get; set; } = "Dashboard";
    public string Action { get; set; } = "Index";
}
