using System;
using System.ComponentModel.DataAnnotations;

namespace SparkVenture.Models;

// One registration-preparation selection per application. This is not a CIPC filing record.
public sealed class RegistrationFinancialYearEnd
{
    [Key]
    public Guid RegistrationApplicationId { get; set; }

    [Range(1, 12)]
    public int YearEndMonth { get; set; }
}
