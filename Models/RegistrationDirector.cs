#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationDirector
        {
            public Guid RegistrationDirectorId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public string FullName { get; set; } = "";
            public string RoleTitle { get; set; } = "";
            public string? Email { get; set; }
            public string? ContactNumber { get; set; }
            public string? IdentityNumberEncrypted { get; set; }
            public RegistrationApplication? RegistrationApplication { get; set; }
        }
}
