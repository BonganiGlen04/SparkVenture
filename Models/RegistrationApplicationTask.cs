#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationApplicationTask
        {
            public Guid RegistrationApplicationTaskId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public Guid? RegistrationTaskTemplateId { get; set; }
            public Guid? AssignedToUserId { get; set; }
            public string TaskKey { get; set; } = "";
            public string TitleSnapshot { get; set; } = "";
            public string? InstructionsSnapshot { get; set; }
            public string Status { get; set; } = "Pending";
            public DateTime? CompletedAtUtc { get; set; }
            public RegistrationApplication? RegistrationApplication { get; set; }
            public RegistrationTaskTemplate? Template { get; set; }
            public ApplicationUser? AssignedToUser { get; set; }
        }
}
