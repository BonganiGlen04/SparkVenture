#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class TenderApplicationDocument
        {
            public Guid TenderApplicationDocumentId { get; set; }
            public Guid TenderApplicationId { get; set; }
            public Guid DocumentId { get; set; }
            public Guid BusinessId { get; set; }
            public string DocumentPurpose { get; set; } = "";
            public bool IsRequiredEvidence { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public TenderApplication? TenderApplication { get; set; }
            public Document? Document { get; set; }
            public Business? Business { get; set; }
        }
}
