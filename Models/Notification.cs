#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Notification
        {
            public Guid NotificationId { get; set; }
            public Guid UserId { get; set; }
            public Guid? BusinessId { get; set; }
            public Guid? NotificationTemplateId { get; set; }
            public string Channel { get; set; } = "";
            public string Title { get; set; } = "";
            public string Body { get; set; } = "";
            public DateTime CreatedAtUtc { get; set; }
            public DateTime? ReadAtUtc { get; set; }
            public ApplicationUser? User { get; set; }
            public Business? Business { get; set; }
            public NotificationTemplate? Template { get; set; }
        }
}
