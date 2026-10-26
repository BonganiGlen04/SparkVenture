using System;
using System.Collections.Generic;

namespace SparkVenture.ViewModels;

public sealed class AdminUsersIndexVm
{
    public string Search { get; set; } = "";
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<AdminUserRowVm> Rows { get; set; } = new();
}

public sealed class AdminUserRowVm
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = "";
    public string Roles { get; set; } = "";
    public string Email { get; set; } = "";
    public bool EmailConfirmed { get; set; }
}

public sealed class AdminBusinessesIndexVm
{
    public string Search { get; set; } = "";
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public List<AdminBusinessRowVm> Rows { get; set; } = new();
}

public sealed class AdminBusinessRowVm
{
    public Guid BusinessId { get; set; }
    public string TradingName { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public int ActiveMembers { get; set; }
}
