using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Models.Entities;

// One browsable item in the Explorer (a movie, a game, a series...)
public class Content
{
    public int ContentId { get; set; }

    public int CategoryId { get; set; }                  // FK -> Category

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public ContentType Type { get; set; }                // Movie / Series / Game...

    [StringLength(100)]
    public string? Genre { get; set; }                   // e.g. "Action, Shonen"

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? ReleaseDate { get; set; }           // nullable = "release date unknown"

    public int PopularityScore { get; set; }             // admin-managed ranking number

    public int ViewCount { get; set; }                   // incremented on every detail visit

    [StringLength(300)]
    public string? ThumbnailUrl { get; set; }            // card cover image

    [StringLength(200)]
    public string? Artist { get; set; }                  // Singer / Band / Composer for Music

    [StringLength(200)]
    public string? AlbumName { get; set; }               // Album or OST name for Music

    [StringLength(500)]
    public string? OfficialWebsiteUrl { get; set; }      // Official game / movie site link

    [StringLength(500)]
    public string? PlayableGameUrl { get; set; }         // Legal HTML5 browser game embed link

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<ContentTag> ContentTags { get; set; } = new List<ContentTag>();
}
