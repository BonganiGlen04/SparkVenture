using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels;

public class OpportunityCardVm
{
    public Guid OpportunityId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string SourceName { get; set; } = "";
    public string? SourceWebsite { get; set; }
    public DateTime? OpensAtUtc { get; set; }
    public DateTime? ClosesAtUtc { get; set; }
    public bool IsSaved { get; set; }
    public bool IsDemo { get; set; }
    public bool IsOpen => Status == "Open" &&
        (OpensAtUtc == null || OpensAtUtc <= DateTime.UtcNow) &&
        (ClosesAtUtc == null || ClosesAtUtc >= DateTime.UtcNow);
}
public class OpportunitiesIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string Search { get; set; } = "";
    public string Type { get; set; } = "All";
    public string State { get; set; } = "Open";
    public bool CanManage { get; set; }
    public List<OpportunityCardVm> Opportunities { get; set; } = new();
}
public class OpportunityRequirementVm
{
    public string Type { get; set; } = "";
    public string Operator { get; set; } = "";
    public string Value { get; set; } = "";
    public bool IsMandatory { get; set; }
}
public class OpportunityDetailsVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public OpportunityCardVm Opportunity { get; set; } = new();
    public List<OpportunityRequirementVm> Requirements { get; set; } = new();
    public Guid? TenderApplicationId { get; set; }
}
public class RecommendationVm
{
    public OpportunityCardVm Opportunity { get; set; } = new();
    public decimal IndicativeScore { get; set; }
    public string Readiness { get; set; } = "";
    public string Explanation { get; set; } = "";
    public bool MandatoryCriteriaPassed { get; set; }
    public List<string> Gaps { get; set; } = new();
    public int UnknownChecks { get; set; }
}
public class RecommendationsIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public List<RecommendationVm> Matches { get; set; } = new();
    public DateTime? LastSavedRunUtc { get; set; }
}
public class SavedOpportunityRowVm
{
    public Guid SavedOpportunityId { get; set; }
    public string? Notes { get; set; }
    public OpportunityCardVm Opportunity { get; set; } = new();
}
public class SavedOpportunitiesIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public List<SavedOpportunityRowVm> Items { get; set; } = new();
}
public class TenderRowVm
{
    public Guid TenderApplicationId { get; set; }
    public Guid OpportunityId { get; set; }
    public string Title { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? DeadlineUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
}
public class TenderApplicationsIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public List<TenderRowVm> Items { get; set; } = new();
}
public class TenderDocumentRowVm
{
    public Guid TenderApplicationDocumentId { get; set; }
    public string OriginalFileName { get; set; } = "";
    public string Purpose { get; set; } = "";
    public bool HasStoredContent { get; set; }
}
public class TenderDetailsVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanManage { get; set; }
    public TenderRowVm Application { get; set; } = new();
    public string? SourceWebsite { get; set; }
    public string SourceName { get; set; } = "";
    public bool IsDemo { get; set; }
    public List<OpportunityRequirementVm> Requirements { get; set; } = new();
    public List<TenderDocumentRowVm> Documents { get; set; } = new();
    public List<SelectListItem> AvailableDocuments { get; set; } = new();
}
public class OpportunityTrackingVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public int DueSoon { get; set; }
    public List<SavedOpportunityRowVm> Saved { get; set; } = new();
    public List<TenderRowVm> Applications { get; set; } = new();
}
