using System;
using System.Collections.Generic;

namespace SparkVenture.ViewModels;

public sealed class RegistrationApplicationsIndexViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public List<RegistrationApplicationListItemViewModel> Applications { get; set; } = new();
}

public sealed class RegistrationApplicationListItemViewModel
{
    public Guid RegistrationApplicationId { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStepKey { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class RegistrationApplicationDetailsViewModel
{
    public Guid RegistrationApplicationId { get; set; }
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CurrentStepKey { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public List<RegistrationStepViewModel> Steps { get; set; } = new();
    public int CompletedSteps => Steps.FindAll(s => s.Status == "Completed").Count;
    public int TotalSteps => Steps.Count;
}

public sealed class RegistrationStepViewModel
{
    public string StepKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
