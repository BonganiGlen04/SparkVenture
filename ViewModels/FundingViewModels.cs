using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels;

public sealed class FundingProviderRowVm
{
    public Guid FundingProviderId { get; set; }
    public string Name { get; set; } = "";
    public string ProviderType { get; set; } = "";
    public string? WebsiteUrl { get; set; }
    // From seeded reference data: NOT proof of an active fund or currently open applications.
    public bool IsDemo => (WebsiteUrl ?? "").Contains("example.org", StringComparison.OrdinalIgnoreCase);
}
public sealed class FundingProvidersIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public string Search { get; set; } = "";
    public List<FundingProviderRowVm> Providers { get; set; } = new();
}

public sealed class FundingReadinessQuestionVm
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Help { get; set; } = "";
    public string Answer { get; set; } = "NotSure";
}
public sealed class FundingReadinessIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public Guid? LatestAssessmentId { get; set; }
    public DateTime? LastCheckedUtc { get; set; }
    public decimal? LastScore { get; set; }
    public List<FundingReadinessQuestionVm> Questions { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
}
public sealed class FundingReadinessSubmitVm
{
    public Guid BusinessId { get; set; }
    [Required]
    public string Registration { get; set; } = "NotSure";
    [Required]
    public string BankAccount { get; set; } = "NotSure";
    [Required]
    public string Tax { get; set; } = "NotSure";
    [Required]
    public string Bookkeeping { get; set; } = "NotSure";
    [Required]
    public string BusinessPlan { get; set; } = "NotSure";
}

public sealed class FundingApplicationRowVm
{
    public Guid FundingApplicationId { get; set; }
    public Guid? FundingProviderId { get; set; }
    public string ProviderName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal RequestedAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
public sealed class FundingApplicationsIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public List<FundingApplicationRowVm> Applications { get; set; } = new();
}
public sealed class FundingApplicationCreateVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    [Required(ErrorMessage = "Select a funding provider.")]
    public Guid FundingProviderId { get; set; }
    [Range(typeof(decimal), "100", "1000000000", ErrorMessage = "Enter an amount between R100 and R1 billion.")]
    public decimal RequestedAmount { get; set; }
    public List<SelectListItem> ProviderOptions { get; set; } = new();
}
public sealed class FundingApplicationDocumentVm
{
    public Guid FundingApplicationDocumentId { get; set; }
    public string FileName { get; set; } = "";
    public string Purpose { get; set; } = "";
}
public sealed class FundingApplicationDetailsVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public FundingApplicationRowVm Application { get; set; } = new();
    public string? WebsiteUrl { get; set; }
    public bool IsDemoProvider { get; set; }
    public List<FundingApplicationDocumentVm> Documents { get; set; } = new();
    public List<SelectListItem> AvailableDocuments { get; set; } = new();
}
