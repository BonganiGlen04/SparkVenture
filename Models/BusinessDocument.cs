#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessDocument
        {
            public Guid BusinessId { get; set; }
            public Guid DocumentId { get; set; }
            public string DocumentPurpose { get; set; } = "";
            public bool IsPrimary { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public Document? Document { get; set; }
        }
}
