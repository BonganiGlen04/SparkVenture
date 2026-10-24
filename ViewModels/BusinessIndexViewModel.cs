using System;
using System.Collections.Generic;
using System.Linq;

namespace SparkVenture.ViewModels
{
    public sealed class BusinessIndexViewModel
    {
        public List<BusinessCardViewModel> Businesses { get; set; } = new();

        public int TotalBusinesses => Businesses.Count;
        public int OwnedBusinesses => Businesses.Count(b => b.IsOwner);
        public int TeamBusinesses => TotalBusinesses - OwnedBusinesses;
    }

    public sealed class BusinessCardViewModel
    {
        public Guid BusinessId { get; set; }
        public string TradingName { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string BusinessType { get; set; } = "Not specified";
        public string BusinessStage { get; set; } = "Not specified";
        public string Sector { get; set; } = "Not specified";
        public string Province { get; set; } = "Not specified";
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }

        public bool IsOwner => string.Equals(Role, "Owner", StringComparison.OrdinalIgnoreCase);
        public bool CanEdit => IsOwner || string.Equals(Role, "Manager", StringComparison.OrdinalIgnoreCase);
    }
}
