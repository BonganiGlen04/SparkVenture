using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparkVenture.ViewModels;

public sealed class BusinessDocumentsIndexViewModel
{
    public Guid BusinessId { get; set; }
    public string BusinessName { get; set; } = "";
    public string CurrentUserRole { get; set; } = "";
    public bool CanUpload { get; set; }
    public List<BusinessDocumentListItemViewModel> Documents { get; set; } = new();
}

public sealed class BusinessDocumentListItemViewModel
{
    public Guid BusinessDocumentId { get; set; }
    public Guid DocumentId { get; set; }
    public string OriginalFileName { get; set; } = "";
    public string DocumentPurpose { get; set; } = "";
    public string ScanStatus { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsPrimary { get; set; }
    public bool HasStoredContent { get; set; }
}

public sealed class UploadBusinessDocumentViewModel
{
    [Required]
    public Guid BusinessId { get; set; }

    [Required(ErrorMessage = "Choose the document type.")]
    [StringLength(50)]
    [Display(Name = "Document type")]
    public string DocumentPurpose { get; set; } = "";

    [Required(ErrorMessage = "Choose a file to upload.")]
    [Display(Name = "File")]
    public IFormFile? File { get; set; }

    [ValidateNever]
    public string BusinessName { get; set; } = "";

    [ValidateNever]
    public List<SelectListItem> DocumentPurposes { get; set; } = new();
}
