#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class NotificationTemplate
        {
            public Guid NotificationTemplateId { get; set; }
            public string TemplateKey { get; set; } = "";
            public string Channel { get; set; } = "";
            public string Locale { get; set; } = "en-ZA";
            public string TitleTemplate { get; set; } = "";
            public string BodyTemplate { get; set; } = "";
            public bool IsActive { get; set; } = true;
        }
}
