#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationProposedName
        {
            public Guid RegistrationProposedNameId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public string ProposedName { get; set; } = "";
            public int PriorityOrder { get; set; }
            public string AvailabilityStatus { get; set; } = "Unknown";
            public RegistrationApplication? RegistrationApplication { get; set; }
        }
}
