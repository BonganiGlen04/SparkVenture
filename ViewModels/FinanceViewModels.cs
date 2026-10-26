using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels;

public sealed class CustomerListItemVm
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string CustomerType { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
public sealed class CustomersIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanEdit { get; set; }
    public string Search { get; set; } = "";
    public List<CustomerListItemVm> Customers { get; set; } = new();
}
public sealed class CustomerFormVm
{
    public Guid BusinessId { get; set; }
    public Guid? CustomerId { get; set; }
    public string BusinessName { get; set; } = "";
    [Required, StringLength(150)] public string DisplayName { get; set; } = "";
    [Required] public string CustomerType { get; set; } = "Individual";
    [EmailAddress, StringLength(150)] public string? Email { get; set; }
    [StringLength(30)] public string? Phone { get; set; }
}
public sealed class InvoiceLineVm
{
    [Required, StringLength(200)] public string Description { get; set; } = "";
    [Range(typeof(decimal), "0.0001", "99999999")] public decimal Quantity { get; set; } = 1;
    [Range(typeof(decimal), "0", "99999999")] public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
public sealed class InvoiceFormVm
{
    public Guid BusinessId { get; set; }
    public Guid? InvoiceId { get; set; }
    public string BusinessName { get; set; } = "";
    [Required] public Guid CustomerId { get; set; }
    [DataType(DataType.Date)] public DateTime IssueDate { get; set; } = DateTime.UtcNow.Date;
    [DataType(DataType.Date)] public DateTime DueDate { get; set; } = DateTime.UtcNow.Date.AddDays(14);
    [Range(typeof(decimal), "0", "15")] public decimal TaxRatePercent { get; set; }
    public List<InvoiceLineVm> Lines { get; set; } = new() { new InvoiceLineVm() };
    public List<SelectListItem> Customers { get; set; } = new();
}
public sealed class InvoiceRowVm
{
    public Guid InvoiceId { get; set; }
    public string Number { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding => Math.Max(0, Total - Paid);
}
public sealed class InvoicesIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanEdit { get; set; }
    public List<InvoiceRowVm> Invoices { get; set; } = new();
}
public sealed class InvoiceDetailsVm
{
    public Guid BusinessId { get; set; }
    public Guid InvoiceId { get; set; }
    public string BusinessName { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Number { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public decimal Paid { get; set; }
    public decimal Outstanding => Math.Max(0, Total - Paid);
    public bool CanEdit { get; set; }
    public List<InvoiceLineVm> Lines { get; set; } = new();
}
public sealed class PaymentRecordVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    [Required] public Guid InvoiceId { get; set; }
    [Range(typeof(decimal), "0.01", "999999999")] public decimal Amount { get; set; }
    [Required] public string PaymentMethod { get; set; } = "EFT";
    [Required, StringLength(50)] public string Reference { get; set; } = "";
    [DataType(DataType.Date)] public DateTime ReceivedOn { get; set; } = DateTime.UtcNow.Date;
    public bool ConfirmReceived { get; set; }
    public List<SelectListItem> Invoices { get; set; } = new();
}
public sealed class PaymentRowVm
{
    public Guid PaymentId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public DateTime ReceivedOn { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = "";
    public string Method { get; set; } = "";
    public string Status { get; set; } = "";
    public bool CanReverse { get; set; }
}
public sealed class PaymentsIndexVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public bool CanEdit { get; set; }
    public List<PaymentRowVm> Payments { get; set; } = new();
}
public sealed class MonthlyCashVm
{
    public string Month { get; set; } = "";
    public decimal MoneyIn { get; set; }
    public decimal MoneyOut { get; set; }
    public decimal Net => MoneyIn - MoneyOut;
}
public sealed class FinanceInsightsVm
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public decimal OutstandingInvoices { get; set; }
    public decimal OverdueInvoices { get; set; }
    public int OverdueCount { get; set; }
    public decimal CurrentMonthMoneyIn { get; set; }
    public decimal CurrentMonthMoneyOut { get; set; }
    public decimal? EstimatedNextMonthNet { get; set; }
    public int ForecastMonthsUsed { get; set; }
    public List<MonthlyCashVm> Months { get; set; } = new();
    public List<string> NextActions { get; set; } = new();
}
