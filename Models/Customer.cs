#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Customer
        {
            public Guid CustomerId { get; set; }
            public Guid BusinessId { get; set; }
            public string CustomerType { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public decimal? AvgDaysToPay { get; set; }       // v3.1 cached metric
            public decimal? LateInvoiceRate { get; set; }   // v3.1 ratio from 0 to 1
            public Business? Business { get; set; }
            public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
            public ICollection<CashbookEntry> CashbookEntries { get; set; } = new List<CashbookEntry>();
        }
}
