
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels
{
    public class CreateBusinessViewModel
    {
        [Required(ErrorMessage = "Business name is required.")]
        [StringLength(150)]
        [Display(Name = "Business / Trading Name")]
        public string TradingName { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Registered Legal Name")]
        public string? LegalName { get; set; }

        [StringLength(30)]
        [Display(Name = "CIPC Registration Number")]
        public string? RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Please select a business type.")]
        [Display(Name = "Business Type")]
        public Guid? BusinessTypeId { get; set; }

        [Required(ErrorMessage = "Please select a business stage.")]
        [Display(Name = "Business Stage")]
        public Guid? BusinessStageId { get; set; }

        [Required(ErrorMessage = "Please select a business sector.")]
        [Display(Name = "Business Sector")]
        public Guid? SectorId { get; set; }

        [Required(ErrorMessage = "Please select your province.")]
        [Display(Name = "Province")]
        public Guid? ProvinceId { get; set; }

        // Dropdown options populated by the controller

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
