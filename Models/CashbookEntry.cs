#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class CashbookEntry
        {
            public Guid CashbookEntryId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? CustomerId { get; set; }
            public Guid? InvoiceId { get; set; }
            public Guid? PaymentAllocationId { get; set; }
            public Guid? ExpenseCategoryId { get; set; }
            public Guid? ReversesCashbookEntryId { get; set; }
            public Guid CreatedByUserId { get; set; }
            public string Direction { get; set; } = "";
            public string EntryType { get; set; } = "";
            public string Status { get; set; } = "Posted";
            public decimal Amount { get; set; }
            public DateTime EntryDate { get; set; }
            public string? Description { get; set; }
            public string? Reference { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public Customer? Customer { get; set; }
            public Invoice? Invoice { get; set; }
            public PaymentAllocation? PaymentAllocation { get; set; }
            public ExpenseCategory? ExpenseCategory { get; set; }
            public ApplicationUser? CreatedByUser { get; set; }
            public CashbookEntry? ReversesCashbookEntry { get; set; }
            public ICollection<CashbookEntryDocument> Documents { get; set; } = new List<CashbookEntryDocument>();
        }
}
