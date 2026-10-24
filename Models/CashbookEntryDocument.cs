#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class CashbookEntryDocument
        {
            public Guid CashbookEntryDocumentId { get; set; }
            public Guid CashbookEntryId { get; set; }
            public Guid DocumentId { get; set; }
            public Guid BusinessId { get; set; }
            public string DocumentPurpose { get; set; } = "";
            public DateTime CreatedAtUtc { get; set; }
            public CashbookEntry? CashbookEntry { get; set; }
            public Document? Document { get; set; }
            public Business? Business { get; set; }
        }
}
