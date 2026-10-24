#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class DataSubjectRequest
        {
            public Guid DataSubjectRequestId { get; set; }
            public Guid? RequesterUserId { get; set; }
            public Guid? AssignedToUserId { get; set; }
            public string? VerifiedContactReference { get; set; }
            public string RequestType { get; set; } = "";
            public string Status { get; set; } = "Received";
            public DateTime ReceivedAtUtc { get; set; }
            public DateTime? DueAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public ApplicationUser? RequesterUser { get; set; }
            public ApplicationUser? AssignedToUser { get; set; }
        }
}
