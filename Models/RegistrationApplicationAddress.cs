#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class RegistrationApplicationAddress
        {
            public Guid RegistrationApplicationAddressId { get; set; }
            public Guid RegistrationApplicationId { get; set; }
            public Guid ProvinceId { get; set; }
            public string AddressType { get; set; } = "";
            public string AddressLine1 { get; set; } = "";
            public string? AddressLine2 { get; set; }
            public string? City { get; set; }
            public string? PostalCode { get; set; }
            public RegistrationApplication? RegistrationApplication { get; set; }
        }
}
