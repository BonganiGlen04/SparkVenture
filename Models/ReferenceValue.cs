#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class ReferenceValue
        {
            public Guid ReferenceValueId { get; set; }
            public string Category { get; set; } = "";
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; } = true;
        }
}
