#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SparkVenture.Data.Models
{
        public class ExpenseCategory
        {
            public Guid ExpenseCategoryId { get; set; }
            public Guid? BusinessId { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public bool IsActive { get; set; } = true;
            public int SortOrder { get; set; }
            public Business? Business { get; set; }
        }
}
