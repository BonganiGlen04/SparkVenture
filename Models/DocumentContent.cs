using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparkVenture.Models;

/// <summary>
/// Binary contents of a Document. One row per persisted document.
/// Store only file bytes here; keep metadata in Documents.
/// </summary>
public sealed class DocumentContent
{
    [Key]
    public Guid DocumentId { get; set; }

    [Required]
    [Column(TypeName = "varbinary(max)")]
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
}
