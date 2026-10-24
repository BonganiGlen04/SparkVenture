using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels
{
    public sealed class EditBusinessViewModel
    {
        public Guid BusinessId { get; set; }

        [Required(ErrorMessage = "Enter the trading name.")]
        [StringLength(150)]
        [Display(Name = "Business / Trading Name")]
        public string TradingName { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Registered Legal Name")]
        public string? LegalName { get; set; }

        [StringLength(30)]
        [Display(Name = "CIPC Registration Number")]
        public string? RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Choose a business type.")]
        public Guid? BusinessTypeId { get; set; }

        [Required(ErrorMessage = "Choose a business stage.")]
        public Guid? BusinessStageId { get; set; }

        [Required(ErrorMessage = "Choose a sector.")]
        public Guid? SectorId { get; set; }

        [Required(ErrorMessage = "Choose a province.")]
        public Guid? ProvinceId { get; set; }

        [ValidateNever]
        public List<SelectListItem> BusinessTypes { get; set; } = new();

        [ValidateNever]
        public List<SelectListItem> BusinessStages { get; set; } = new();

        [ValidateNever]
        public List<SelectListItem> Sectors { get; set; } = new();

        [ValidateNever]
        public List<SelectListItem> Provinces { get; set; } = new();
    }
}
