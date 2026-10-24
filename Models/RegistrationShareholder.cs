#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationShareholder
        {
            public Guid RegistrationShareholderId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public string ShareholderType { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public string? ShareClass { get; set; }
            public decimal? SharePercentage { get; set; }
            public string? IdentityNumberEncrypted { get; set; }
            public RegistrationApplication? RegistrationApplication { get; set; }
        }
}
