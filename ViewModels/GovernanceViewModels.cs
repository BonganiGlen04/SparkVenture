using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SparkVenture.ViewModels;

public sealed class AdminOverviewVm
{
    public int BusinessCount { get; set; }
    public int UserCount { get; set; }
    public int ReferenceCount { get; set; }
    public int PendingRequestCount { get; set; }
    public int AuditCount { get; set; }
}
public sealed class AdminReferenceRowVm
{
    public Guid Id { get; set; }
    public string Category { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
public sealed class AdminReferencesVm
{
    public string Category { get; set; } = "";
    public List<string> Categories { get; set; } = new();
    public List<AdminReferenceRowVm> Rows { get; set; } = new();
}
public sealed class AdminReferenceEditVm
{
    [Required] public Guid Id { get; set; }
    [Required, StringLength(100, MinimumLength = 2)] public string Name { get; set; } = "";
    [StringLength(400)] public string? Description { get; set; }
    [Range(0, 10000)] public int SortOrder { get; set; }
}
public sealed class AdminSectorRowVm
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}
public sealed class AdminSectorEditVm
{
    [Required] public Guid Id { get; set; }
    [Required, StringLength(100, MinimumLength=2)] public string Name { get; set; } = "";
    [StringLength(200)] public string? Description { get; set; }
}
public sealed class AdminSectorsVm
{
    public List<AdminSectorRowVm> Rows { get; set; } = new();
}
public sealed class AdminPublishNoticeVm
{
    [Required, RegularExpression(@"^[a-zA-Z0-9._-]{2,30}$")]
    public string VersionLabel { get; set; } = "";
    [Required, StringLength(30000, MinimumLength = 200)]
    public string ContentText { get; set; } = "";
}
public sealed class AdminNoticeRowVm
{
    public string VersionLabel { get; set; } = "";
    public DateTime PublishedAtUtc { get; set; }
}
public sealed class AdminNoticesVm
{
    public AdminPublishNoticeVm Draft { get; set; } = new();
    public List<AdminNoticeRowVm> History { get; set; } = new();
}
public sealed class AdminRequestRowVm
{
    public Guid Id { get; set; }
    public Guid RequesterUserId { get; set; }
    public string RequesterName { get; set; } = "";
    public string RequesterEmail { get; set; } = "";
    public Guid? AssignedToUserId { get; set; }
    public string AssignedToName { get; set; } = "";
    public string RequestType { get; set; } = "";
    public string Status { get; set; } = "";
    public bool ContactReferenceAvailable { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
}
public sealed class AdminRequestsVm
{
    public List<AdminRequestRowVm> Rows { get; set; } = new();
}
public sealed class PrivacyNoticeVm
{
    public string VersionLabel { get; set; } = "";
    public DateTime? PublishedAtUtc { get; set; }
    public string Text { get; set; } = "";
    public bool IsDemoOrMissing { get; set; }
}
public sealed class PrivacyRequestRowVm
{
    public string RequestType { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime DueAtUtc { get; set; }
}
public sealed class PrivacyIndexVm
{
    public PrivacyNoticeVm Notice { get; set; } = new();
    public bool? MarketingChoice { get; set; }
    public List<PrivacyRequestRowVm> Requests { get; set; } = new();
}
public sealed class PrivacyRequestInputVm
{
    [Required, RegularExpression("^(Access|Correction|Deletion)$", ErrorMessage="Choose a supported request type.")]
    public string RequestType { get; set; } = "Access";
}
public sealed class AuditLogRowVm
{
    public Guid AuditLogId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public Guid? ActorUserId { get; set; }
    public Guid? BusinessId { get; set; }
    public string ActionCode { get; set; } = "";
    public string ActionDescription { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string EntityDescription { get; set; } = "";
    public string ActorName { get; set; } = "";
    public string ActorEmail { get; set; } = "";
    public string BusinessName { get; set; } = "";
}
public sealed class AuditLogsIndexVm
{
    public string ActionFilter { get; set; } = "";
    public int Page { get; set; }
    public int TotalRows { get; set; }
    public int PageSize { get; set; } = 30;
    public List<AuditLogRowVm> Rows { get; set; } = new();
}

public sealed class AdminRetentionRowVm
{
    public string EntityType { get; set; } = "";
    public int RetentionDays { get; set; }
    public string Action { get; set; } = "";
    public bool IsActive { get; set; }
}
