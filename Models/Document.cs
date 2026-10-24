#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class Document
        {
            public Guid DocumentId { get; set; }
            public Guid? BusinessId { get; set; }
            public Guid UploadedByUserId { get; set; }
            public string OriginalFileName { get; set; } = "";
            public string StorageKey { get; set; } = "";
            public string DetectedContentType { get; set; } = "";
            public string FileExtension { get; set; } = "";
            public long SizeBytes { get; set; }
            public string? ContentHash { get; set; }
            public string ScanStatus { get; set; } = "Pending";
            public DateTime CreatedAtUtc { get; set; }
            public Business? Business { get; set; }
            public ApplicationUser? UploadedByUser { get; set; }
        }
}
