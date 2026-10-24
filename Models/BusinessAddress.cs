#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class BusinessAddress
        {
            public Guid BusinessAddressId { get; set; }
            public Guid BusinessId { get; set; }
            public Guid ProvinceId { get; set; }
            public string AddressType { get; set; } = "";
            public string AddressLine1 { get; set; } = "";
            public string? AddressLine2 { get; set; }
            public string? Suburb { get; set; }
            public string? City { get; set; }
            public string? PostalCode { get; set; }
            public Business? Business { get; set; }
        }
}
