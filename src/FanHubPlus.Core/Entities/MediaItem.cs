using System.ComponentModel.DataAnnotations;
using FanHubPlus.Core.Enums;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// One embedded media file attached to a content item (trailer, video, audio).
/// </summary>
public class MediaItem
{
    [Key]
    public int MediaId { get; set; }

    public int ContentId { get; set; }

    public MediaType MediaType { get; set; }

    [Required]
    [StringLength(500)]
    public string EmbedUrl { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Tag { get; set; }

    public int? DurationSeconds { get; set; }

    public int OrderIndex { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Content Content { get; set; } = null!;
}