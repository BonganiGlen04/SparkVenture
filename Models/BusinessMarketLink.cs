#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessMarketLink
        {
            public Guid BusinessMarketLinkId { get; set; }
            public Guid BusinessId { get; set; }
            public string MarketName { get; set; } = "";
            public string Url { get; set; } = "";
            public Business? Business { get; set; }
        }
}
