#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public enum BusinessInsightType
        {
            CashShortfall,
            LatePaymentRisk,
            DeadlineSoon,
            CredentialExpiring,
            NextAction
        }
}
