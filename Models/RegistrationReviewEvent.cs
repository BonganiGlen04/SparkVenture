#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationReviewEvent
        {
            public Guid RegistrationReviewEventId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public Guid? ActorUserId { get; set; }
            public string? FromStatus { get; set; }
            public string ToStatus { get; set; } = "";
            public string EventType { get; set; } = "";
            public string? Comment { get; set; }
            public DateTime OccurredAtUtc { get; set; }
            public RegistrationApplication? RegistrationApplication { get; set; }
            public ApplicationUser? ActorUser { get; set; }
        }
}
