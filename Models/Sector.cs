#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Sector
        {
            public Guid SectorId { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public ICollection<Business> Businesses { get; set; } = new List<Business>();
            public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        }
}
