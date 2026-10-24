#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class ApplicationUser : IdentityUser<Guid>
        {
            public UserProfile? Profile { get; set; }
            public ICollection<BusinessMember> BusinessMemberships { get; set; } = new List<BusinessMember>();
        }
}
