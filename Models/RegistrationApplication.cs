#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationApplication
        {
            public Guid RegistrationApplicationId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid CreatedByUserId { get; set; }
            public Guid? AssignedReviewerUserId { get; set; }
            public string WorkflowType { get; set; } = "CompanyRegistration";
            public string Status { get; set; } = "Draft";
            public string? CurrentStepKey { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? CreatedByUser { get; set; }
            public ApplicationUser? AssignedReviewerUser { get; set; }
            public ICollection<RegistrationProposedName> ProposedNames { get; set; } = new List<RegistrationProposedName>();
            public ICollection<RegistrationDirector> Directors { get; set; } = new List<RegistrationDirector>();
            public ICollection<RegistrationShareholder> Shareholders { get; set; } = new List<RegistrationShareholder>();
            public ICollection<RegistrationApplicationAddress> Addresses { get; set; } = new List<RegistrationApplicationAddress>();
            public ICollection<RegistrationApplicationTask> Tasks { get; set; } = new List<RegistrationApplicationTask>();
            public ICollection<RegistrationReviewEvent> ReviewEvents { get; set; } = new List<RegistrationReviewEvent>();
        }
}
