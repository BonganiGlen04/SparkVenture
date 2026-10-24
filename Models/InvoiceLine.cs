#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class InvoiceLine
        {
            public Guid InvoiceLineId { get; set; }
            public Guid InvoiceId { get; set; }
            public Guid BusinessId { get; set; }
            public string Description { get; set; } = "";
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }
            public Invoice? Invoice { get; set; }
        }
}
