#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class TenderApplication
        {
            public Guid TenderApplicationId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid OpportunityId { get; set; }
            public Guid CreatedByUserId { get; set; }
            public string Status { get; set; } = "Draft";
            public DateTime? SubmissionDeadlineUtc { get; set; }
            public DateTime? SubmittedAtUtc { get; set; }
            public Business? Business { get; set; }
            public Opportunity? Opportunity { get; set; }
            public ApplicationUser? CreatedByUser { get; set; }
            public ICollection<TenderApplicationDocument> Documents { get; set; } = new List<TenderApplicationDocument>();
        }
}
