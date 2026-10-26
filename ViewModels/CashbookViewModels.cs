using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels;

public sealed class CashbookEntryRowViewModel
{
    public Guid CashbookEntryId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Direction { get; set; } = "";
    public string EntryType { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string? Reference { get; set; }
    public string? CategoryName { get; set; }
    public Guid? ReversesCashbookEntryId { get; set; }
    public bool CanReverse { get; set; }
}

public sealed class CashbookIndexViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool CanEdit { get; set; }
    public string Month { get; set; } = "";
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public decimal NetMovement => MoneyIn - MoneyOut;
    public int TotalEntries { get; set; }
    public List<CashbookEntryRowViewModel> Entries { get; set; } = new();
}

public sealed class CashbookCreateViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";

    [Required(ErrorMessage = "Select income or expense.")]
    public string Direction { get; set; } = "Out";

    [Required, Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Amount { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.Date;

    [Required, StringLength(200, MinimumLength = 3)]
    public string Description { get; set; } = "";

    [StringLength(50)]
    public string? Reference { get; set; }

    public Guid? ExpenseCategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
}

public sealed class CashbookEntryDetailsViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanEdit { get; set; }
    public CashbookEntryRowViewModel Entry { get; set; } = new();
}

public sealed class CashbookCategoryViewModel
{
    public Guid ExpenseCategoryId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public bool IsGlobal { get; set; }
}

public sealed class CashbookCategoriesViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanEdit { get; set; }
    public List<CashbookCategoryViewModel> Categories { get; set; } = new();

    [Required, StringLength(100, MinimumLength = 2)]
    public string NewCategoryName { get; set; } = "";

    [Required, StringLength(30, MinimumLength = 2)]
    [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Use letters, numbers, _ or -.")]
    public string NewCategoryCode { get; set; } = "";
}

public sealed class CashbookMonthSummaryViewModel
{
    public string Month { get; set; } = "";
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Net => Income - Expenses;
    public int IncomeWidth { get; set; }
    public int ExpensesWidth { get; set; }
}

public sealed class CashbookCategoryTotalViewModel
{
    public string Category { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class CashbookReportViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public List<CashbookMonthSummaryViewModel> Months { get; set; } = new();
    public List<CashbookCategoryTotalViewModel> ExpenseCategories { get; set; } = new();
    public decimal SixMonthIncome { get; set; }
    public decimal SixMonthExpenses { get; set; }
    public decimal SixMonthNet => SixMonthIncome - SixMonthExpenses;
}
