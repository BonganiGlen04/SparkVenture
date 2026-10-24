#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class InvoiceNumberSequence
        {
            public Guid BusinessId { get; set; }
            public string Prefix { get; set; } = "INV-";
            public long NextNumber { get; set; } = 1;
            public byte[]? RowVersion { get; set; }
            public Business? Business { get; set; }
        }
}
