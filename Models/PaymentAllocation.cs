#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class PaymentAllocation
        {
            public Guid PaymentAllocationId { get; set; }
            public Guid PaymentTransactionId { get; set; }
            public Guid InvoiceId { get; set; }
            public Guid BusinessId { get; set; }
            public decimal AllocatedAmount { get; set; }
            public string AllocationStatus { get; set; } = "Allocated";
            public DateTime CreatedAtUtc { get; set; }
            public DateTime? PostedAtUtc { get; set; }
            public PaymentTransaction? PaymentTransaction { get; set; }
            public Invoice? Invoice { get; set; }
            public Business? Business { get; set; }
        }
}
