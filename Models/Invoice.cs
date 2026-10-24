#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Invoice
        {
            public Guid InvoiceId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid CustomerId { get; set; }
            public string InvoiceNumber { get; set; } = "";
            public string Status { get; set; } = "Draft";
            public DateTime IssueDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? FullyPaidAtUtc { get; set; }
            public decimal Subtotal { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public decimal AmountPaid { get; set; }
            public string CurrencyCode { get; set; } = "ZAR";
            public byte[]? RowVersion { get; set; }
            public Business? Business { get; set; }
            public Customer? Customer { get; set; }
            public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
            public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
            public ICollection<CashbookEntry> CashbookEntries { get; set; } = new List<CashbookEntry>();
        }
}
