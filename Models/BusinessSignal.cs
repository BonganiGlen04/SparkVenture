#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessSignal
        {
            public Guid BusinessSignalId { get; set; }
            public Guid BusinessId { get; set; }
            public BusinessSignalType SignalType { get; set; }
            public string Key { get; set; } = "";
            public decimal Weight { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public Business? Business { get; set; }
        }
}
