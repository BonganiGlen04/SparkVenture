
using Microsoft.AspNetCore.Identity;
using SparkVenture.Data.Models;
using SparkVenture.Models;
using System.Collections.Generic;

namespace SparkVenture.Data;

public class ApplicationUser : IdentityUser<Guid>
{
    public UserProfile? Profile { get; set; }
    public ICollection<BusinessMember> BusinessMemberships { get; set; } = new List<BusinessMember>();
}
