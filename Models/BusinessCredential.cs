#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessCredential
        {
            public Guid BusinessCredentialId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid CredentialTypeId { get; set; }
            public Guid? EvidenceDocumentId { get; set; }
            public string? ReferenceNumber { get; set; }
            public string? LevelOrGrade { get; set; }
            public DateTime? IssuedOn { get; set; }
            public DateTime? ExpiresOn { get; set; }
            public string Status { get; set; } = "";
            public Business? Business { get; set; }
            public Document? EvidenceDocument { get; set; }
        }
}
