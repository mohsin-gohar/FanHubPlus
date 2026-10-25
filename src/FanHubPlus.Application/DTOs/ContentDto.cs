using FanHubPlus.Core.Enums;

namespace FanHubPlus.Application.DTOs;

/// <summary>
/// Content payload for cards, lists and detail pages.
/// </summary>
public class ContentDto
{
    public int ContentId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ContentType Type { get; set; }

    public string? Genre { get; set; }

    public string? Description { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public int PopularityScore { get; set; }

    public int ViewCount { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? Artist { get; set; }

    public string? AlbumName { get; set; }

    public string? Director { get; set; }

    public int? RuntimeMinutes { get; set; }

    public string? AgeRating { get; set; }

    public string? Cast { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public List<MediaItemDto> MediaItems { get; set; } = new();
}