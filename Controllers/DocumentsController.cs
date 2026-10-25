using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SparkVenture.Data;
using SparkVenture.Models;
using SparkVenture.Data.Models;
using SparkVenture.ViewModels;

namespace SparkVenture.Controllers;

[Authorize]
public sealed class DocumentsController : Controller
{
    private const long MaxUploadBytes = 10 * 1024 * 1024; // 10 MiB
    private const long MaxRequestBytes = 12 * 1024 * 1024; // multipart overhead

    private static readonly (string Key, string Label)[] Purposes =
    {
        ("CompanyRegistration", "Company registration certificate"),
        ("TaxCompliance", "Tax compliance document"),
        ("BankConfirmation", "Bank confirmation letter"),
        ("BbbeeAffidavit", "B-BBEE affidavit"),
        ("ProofOfAddress", "Proof of address"),
        ("BusinessPlan", "Business plan"),
        ("Other", "Other business document")
    };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        ILogger<DocumentsController> logger)
    {
        _db = db;
        _users = users;
        _logger = logger;
    }

    private bool TryGetCurrentUserId(out Guid userId) =>
        Guid.TryParse(_users.GetUserId(User), out userId);

    private Task<string?> GetDocumentManagerRoleAsync(
        Guid businessId, Guid userId, CancellationToken ct)
        => _db.BusinessMembers.AsNoTracking()
            .Where(m => m.BusinessId == businessId && m.UserId == userId
                && m.MembershipStatus == "Active"
                && (m.BusinessRole == "Owner" || m.BusinessRole == "Manager"))
            .Select(m => m.BusinessRole)
            .FirstOrDefaultAsync(ct);

    // GET /Documents?businessId=<business GUID>
    [HttpGet]
    public async Task<IActionResult> Index(Guid? businessId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Challenge();

        // Sidebar navigation can omit businessId; only auto-select if exactly one is manageable.
        if (businessId is null)
        {
            var allowed = await _db.BusinessMembers.AsNoTracking()
                .Where(m => m.UserId == userId && m.MembershipStatus == "Active"
                    && (m.BusinessRole == "Owner" || m.BusinessRole == "Manager"))
                .Select(m => m.BusinessId).Distinct().Take(2)
                .ToListAsync(cancellationToken);
            if (allowed.Count != 1)
                return RedirectToAction("Index", "Businesses");
            return RedirectToAction(nameof(Index), new { businessId = allowed[0] });
        }

        if (businessId.Value == Guid.Empty) return NotFound();
        var role = await GetDocumentManagerRoleAsync(businessId.Value, userId, cancellationToken);
        if (role is null) return NotFound();

        var name = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == businessId.Value)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);
        if (name is null) return NotFound();

        // Both the link and Document must belong to the same business.
        var records = await (
            from link in _db.BusinessDocuments.AsNoTracking()
            join doc in _db.Documents.AsNoTracking()
                on link.DocumentId equals doc.DocumentId
            where link.BusinessId == businessId.Value && doc.BusinessId == businessId.Value
            orderby link.CreatedAtUtc descending
            select new BusinessDocumentListItemViewModel
            {
                BusinessDocumentId = link.BusinessDocumentId,
                DocumentId = doc.DocumentId,
                OriginalFileName = doc.OriginalFileName,
                DocumentPurpose = link.DocumentPurpose,
                ScanStatus = doc.ScanStatus,
                SizeBytes = doc.SizeBytes,
                CreatedAtUtc = link.CreatedAtUtc,
                IsPrimary = link.IsPrimary,
                HasStoredContent = _db.DocumentContents.Any(c => c.DocumentId == doc.DocumentId)
            }).Take(100).ToListAsync(cancellationToken);

        return View(new BusinessDocumentsIndexViewModel
        {
            BusinessId = businessId.Value,
            BusinessName = name,
            CurrentUserRole = role,
            CanUpload = true,
            Documents = records
        });
    }

    // GET /Documents/Upload?businessId=<business GUID>
    [HttpGet]
    public async Task<IActionResult> Upload(Guid businessId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Challenge();
        if (businessId == Guid.Empty ||
            await GetDocumentManagerRoleAsync(businessId, userId, cancellationToken) is null)
            return NotFound();

        var businessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == businessId)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken);
        if (businessName is null) return NotFound();

        var model = new UploadBusinessDocumentViewModel
        {
            BusinessId = businessId,
            BusinessName = businessName
        };
        PopulatePurposes(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Upload(
        UploadBusinessDocumentViewModel model, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Challenge();
        if (model.BusinessId == Guid.Empty ||
            await GetDocumentManagerRoleAsync(model.BusinessId, userId, cancellationToken) is null)
            return NotFound();

        model.BusinessName = await _db.Businesses.AsNoTracking()
            .Where(b => b.BusinessId == model.BusinessId)
            .Select(b => b.TradingName)
            .FirstOrDefaultAsync(cancellationToken) ?? "";
        if (model.BusinessName.Length == 0) return NotFound();
        PopulatePurposes(model);

        if (!Purposes.Any(p => p.Key == model.DocumentPurpose))
            ModelState.AddModelError(nameof(model.DocumentPurpose), "Choose a valid document type.");

        var file = model.File;
        var extension = "";
        var originalName = "";
        var detectedMimeType = "";

        if (file == null || file.Length == 0)
            ModelState.AddModelError(nameof(model.File), "Please choose a non-empty file.");
        else if (file.Length > MaxUploadBytes)
            ModelState.AddModelError(nameof(model.File), "The file must be 10 MB or smaller.");
        else
        {
            // Browser-supplied filename and ContentType are untrusted.
            originalName = Path.GetFileName(file.FileName.Replace('\\', '/'));
            extension = Path.GetExtension(originalName).ToLowerInvariant();
            if (originalName.Length is < 1 or > 200 ||
                originalName.Any(char.IsControl) ||
                extension is not (".pdf" or ".png" or ".jpg" or ".jpeg"))
            {
                ModelState.AddModelError(nameof(model.File),
                    "Choose a PDF, PNG or JPEG file with a valid filename (maximum 200 characters).");
            }
            else
            {
                var valid = await HasExpectedSignatureAsync(file, extension, cancellationToken);
                if (!valid)
                    ModelState.AddModelError(nameof(model.File), "The file content does not match its type.");
                else
                {
                    if (extension == ".jpeg") extension = ".jpg";
                    detectedMimeType = extension switch
                    {
                        ".pdf" => "application/pdf",
                        ".png" => "image/png",
                        _ => "image/jpeg"
                    };
                }
            }
        }

        if (!ModelState.IsValid) return View(model);

        // With a 10 MiB limit, a byte[] upload is reasonable for the MVP.
        // Do not load DocumentContents in the listing query.
        byte[] fileBytes;
        try
        {
            await using var buffer = new MemoryStream((int)file!.Length);
            await file.CopyToAsync(buffer, cancellationToken);
            if (buffer.Length == 0 || buffer.Length > MaxUploadBytes)
            {
                ModelState.AddModelError(nameof(model.File), "Invalid document size.");
                return View(model);
            }
            fileBytes = buffer.ToArray();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Document read failure for business {BusinessId}", model.BusinessId);
            ModelState.AddModelError("", "The file could not be read. Please try again.");
            return View(model);
        }

        // Demo/MVP mode: no antivirus scan is performed.
        // Preserve the existing valid status without implying the file is clean.
        const string scanStatus = "Pending";

        var documentId = Guid.NewGuid();
        var sha256Hex = Convert.ToHexString(SHA256.HashData(fileBytes));

        try
        {
            var now = DateTime.UtcNow;
            _db.Documents.Add(new Document
            {
                DocumentId = documentId,
                BusinessId = model.BusinessId,
                UploadedByUserId = userId,
                OriginalFileName = originalName,
                // Keep a stable key if the existing schema requires StorageKey.
                // Real bytes now live in DocumentContents, NOT on local disk.
                StorageKey = "sql/" + documentId.ToString("N"),
                DetectedContentType = detectedMimeType,
                FileExtension = extension,
                SizeBytes = fileBytes.LongLength,
                ContentHash = sha256Hex,
                // The signature check is NOT a malware scan.
                ScanStatus = scanStatus,
                CreatedAtUtc = now
            });
            _db.DocumentContents.Add(new DocumentContent
            {
                DocumentId = documentId,
                FileBytes = fileBytes
            });
            _db.BusinessDocuments.Add(new BusinessDocument
            {
                BusinessDocumentId = Guid.NewGuid(),
                BusinessId = model.BusinessId,
                DocumentId = documentId,
                DocumentPurpose = model.DocumentPurpose,
                IsPrimary = false,
                CreatedAtUtc = now
            });
            _db.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                ActorUserId = userId,
                BusinessId = model.BusinessId,
                OccurredAtUtc = now,
                ActionCode = "document.uploaded",
                EntityType = "Document",
                EntityId = documentId.ToString(),
                // Never log document bytes, identity numbers, or raw file content.
                PayloadJson = JsonSerializer.Serialize(new
                {
                    purpose = model.DocumentPurpose,
                    sizeBytes = fileBytes.LongLength,
                    scanStatus,
                    storage = "sql"
                })
            });

            // EF Core saves the parent Document, its binary row, link and audit
            // in a single relational SaveChanges transaction.
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document database save failure for business {BusinessId}", model.BusinessId);
            ModelState.AddModelError("", "Your document could not be saved. Please try again.");
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Document uploaded to SQL Server. This file has not been malware-scanned.";
        return RedirectToAction(nameof(Index), new { businessId = model.BusinessId });
    }

    // GET /Documents/Download?businessId=...&businessDocumentId=...
    // MVP-only: files are not malware-scanned. Serve as forced attachments.
    // For public deployment, add a malware scanning/quarantine workflow.
    [HttpGet]
    public async Task<IActionResult> Download(Guid businessId, Guid businessDocumentId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Challenge();
        if (businessId == Guid.Empty || businessDocumentId == Guid.Empty ||
            await GetDocumentManagerRoleAsync(businessId, userId, cancellationToken) is null)
            return NotFound();

        var result = await (
            from link in _db.BusinessDocuments.AsNoTracking()
            join doc in _db.Documents.AsNoTracking() on link.DocumentId equals doc.DocumentId
            join bytes in _db.DocumentContents.AsNoTracking() on doc.DocumentId equals bytes.DocumentId
            where link.BusinessDocumentId == businessDocumentId &&
                  link.BusinessId == businessId && doc.BusinessId == businessId
            select new
            {
                doc.DocumentId, doc.OriginalFileName,
                doc.SizeBytes, doc.ContentHash, bytes.FileBytes
            }).SingleOrDefaultAsync(cancellationToken);

        // A seeded metadata-only row has no binary row and returns 404.
        if (result is null) return NotFound();

        // Defence in depth: prevent returning truncated or modified content.
        if (result.FileBytes.LongLength != result.SizeBytes ||
            !string.Equals(Convert.ToHexString(SHA256.HashData(result.FileBytes)),
                result.ContentHash, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Content mismatch on document {DocumentId}", result.DocumentId);
            return StatusCode(StatusCodes.Status409Conflict,
                "Document content failed its integrity check.");
        }

        var safeName = Path.GetFileName(result.OriginalFileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(safeName)) safeName = "document.bin";

        _db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            OccurredAtUtc = DateTime.UtcNow, ActionCode = "document.downloaded",
            EntityType = "Document", EntityId = result.DocumentId.ToString(),
            PayloadJson = "{}"
        });
        await _db.SaveChangesAsync(cancellationToken);

        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        // FileDownloadName forces the Content-Disposition attachment response.
        return File(result.FileBytes, "application/octet-stream", safeName);
    }

    // POST /Documents/DeleteLink - unlink only, never delete shared document bytes.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLink(Guid businessId, Guid businessDocumentId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId)) return Challenge();
        if (businessId == Guid.Empty || businessDocumentId == Guid.Empty ||
            await GetDocumentManagerRoleAsync(businessId, userId, cancellationToken) is null)
            return NotFound();

        var link = await (
            from entry in _db.BusinessDocuments
            join doc in _db.Documents on entry.DocumentId equals doc.DocumentId
            where entry.BusinessDocumentId == businessDocumentId &&
                  entry.BusinessId == businessId && doc.BusinessId == businessId
            select entry).SingleOrDefaultAsync(cancellationToken);
        if (link is null) return NotFound();

        _db.BusinessDocuments.Remove(link);
        _db.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(), ActorUserId = userId, BusinessId = businessId,
            OccurredAtUtc = DateTime.UtcNow, ActionCode = "document.business_link_removed",
            EntityType = "BusinessDocument", EntityId = link.BusinessDocumentId.ToString(),
            PayloadJson = JsonSerializer.Serialize(new { documentId = link.DocumentId })
        });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["SuccessMessage"] = "Document removed from this business vault. Its retained source and other links were not deleted.";
        return RedirectToAction(nameof(Index), new { businessId });
    }

    private static void PopulatePurposes(UploadBusinessDocumentViewModel model)
    {
        model.DocumentPurposes = Purposes.Select(p => new SelectListItem
        {
            Value = p.Key,
            Text = p.Label
        }).ToList();
    }

    private static async Task<bool> HasExpectedSignatureAsync(
        Microsoft.AspNetCore.Http.IFormFile file, string ext, CancellationToken ct)
    {
        byte[] header = new byte[8];
        await using var stream = file.OpenReadStream();
        var total = 0;
        while (total < header.Length)
        {
            var count = await stream.ReadAsync(header.AsMemory(total, header.Length - total), ct);
            if (count == 0) break;
            total += count;
        }
        if (ext == ".pdf") return total >= 5 &&
            header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 &&
            header[3] == 0x46 && header[4] == 0x2D;
        if (ext == ".png") return total == 8 &&
            header.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        return total >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
    }
}
