#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class DataRetentionPolicy
        {
            public Guid DataRetentionPolicyId { get; set; }
            public string EntityType { get; set; } = "";
            public int RetentionDays { get; set; }
            public string Action { get; set; } = "Review";
            public bool IsActive { get; set; } = true;
        }
}
