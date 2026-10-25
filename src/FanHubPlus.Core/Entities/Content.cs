using System.ComponentModel.DataAnnotations;
using FanHubPlus.Core.Enums;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// One browsable item in the Explorer (a movie, a game, a series...).
/// </summary>
public class Content
{
    public int ContentId { get; set; }

    public int CategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public ContentType Type { get; set; }

    [StringLength(100)]
    public string? Genre { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int PopularityScore { get; set; }

    public int ViewCount { get; set; }

    [StringLength(300)]
    public string? ThumbnailUrl { get; set; }

    [StringLength(200)]
    public string? Artist { get; set; }

    [StringLength(200)]
    public string? AlbumName { get; set; }

    [StringLength(500)]
    public string? OfficialWebsiteUrl { get; set; }

    [StringLength(500)]
    public string? PlayableGameUrl { get; set; }

    [StringLength(200)]
    public string? Director { get; set; }

    public int? RuntimeMinutes { get; set; }

    [StringLength(20)]
    public string? AgeRating { get; set; }

    [StringLength(100)]
    public string? ProductionCountry { get; set; }

    [StringLength(1000)]
    public string? Cast { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
}