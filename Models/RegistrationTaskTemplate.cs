#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationTaskTemplate
        {
            public Guid RegistrationTaskTemplateId { get; set; }
            public string WorkflowType { get; set; } = "";
            public string StepKey { get; set; } = "";
            public string Title { get; set; } = "";
            public string? Instructions { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; } = true;
        }
}
