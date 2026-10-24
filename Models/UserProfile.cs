#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class UserProfile
        {
            public Guid UserProfileId { get; set; }
            public Guid UserId { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string? PreferredName { get; set; }
            public ApplicationUser? User { get; set; }
        }
}
