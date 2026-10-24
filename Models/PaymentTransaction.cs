#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class PaymentTransaction
        {
            public Guid PaymentTransactionId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid? InitiatedByUserId { get; set; }
            public string ProviderCode { get; set; } = "Manual";
            public string PaymentMethod { get; set; } = "EFT";
            public DateTime ReceivedOn { get; set; }
            public string? Reference { get; set; }
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; } = "ZAR";
            public string Status { get; set; } = "Settled";
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? InitiatedByUser { get; set; }
            public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
        }
}
