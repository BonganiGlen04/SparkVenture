#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class OpportunityRequirement
        {
            public Guid OpportunityRequirementId { get; set; }
            public Guid OpportunityId { get; set; }
            public string RequirementType { get; set; } = "";
            public string Operator { get; set; } = "";
            public string ValueType { get; set; } = "";
            public string Value { get; set; } = "";
            public bool IsMandatory { get; set; }
            public int Weight { get; set; }
            public Opportunity? Opportunity { get; set; }
        }
}
