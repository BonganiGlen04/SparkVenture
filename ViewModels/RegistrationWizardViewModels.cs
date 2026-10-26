using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SparkVenture.ViewModels;

// Posted BusinessId/Status are intentionally absent: authorisation and status come from SQL Server.
public abstract class RegistrationWizardPageViewModel
{
    public Guid ApplicationId { get; set; }
    [ValidateNever]
    public string BusinessName { get; set; } = string.Empty;
    [ValidateNever]
    public string StepTitle { get; set; } = string.Empty;
    [ValidateNever]
    public string StepKey { get; set; } = string.Empty;
    public int StepNumber { get; set; }
}

public sealed class RegistrationYearEndInput : RegistrationWizardPageViewModel
{
    public int? YearEndMonth { get; set; }
}

public sealed class RegistrationNamesInput : RegistrationWizardPageViewModel
{
    [ValidateNever]
    public string TradingName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public bool ConfirmBusinessDetails { get; set; }
    public string? Name1 { get; set; }
    public string? Name2 { get; set; }
    public string? Name3 { get; set; }
}

public sealed class RegistrationDirectorInput
{
    public Guid? Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? ContactNumber { get; set; }
}

public sealed class RegistrationDirectorsInput : RegistrationWizardPageViewModel
{
    public List<RegistrationDirectorInput> Directors { get; set; } = new();
}

public sealed class RegistrationShareholderInput
{
    public Guid? Id { get; set; }
    public string? ShareholderType { get; set; } = "Individual";
    public string? DisplayName { get; set; }
    public decimal? SharePercentage { get; set; }
}

public sealed class RegistrationShareholdersInput : RegistrationWizardPageViewModel
{
    public List<RegistrationShareholderInput> Shareholders { get; set; } = new();
}

public sealed class RegistrationAddressInput : RegistrationWizardPageViewModel
{
    public Guid? ProvinceId { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public List<SelectListItem> Provinces { get; set; } = new();
}

public sealed class RegistrationWizardSummaryViewModel : RegistrationWizardPageViewModel
{
    public Guid BusinessId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? YearEndMonth { get; set; }
    public List<string> ProposedNames { get; set; } = new();
    public List<string> DirectorNames { get; set; } = new();
    public List<RegistrationShareholderSummary> Shareholders { get; set; } = new();
    public string RegisteredAddress { get; set; } = string.Empty;
    public List<RegistrationStepViewModel> Steps { get; set; } = new();
}

public sealed class RegistrationShareholderSummary
{
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
}
