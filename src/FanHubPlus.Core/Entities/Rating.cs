using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// One user's 1-5 star rating of a content item (unique per user+content - see DbContext).
/// </summary>
public class Rating
{
    public int RatingId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int ContentId { get; set; }

    [Range(1, 5)]
    public int Stars { get; set; }

    [StringLength(1000)]
    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Content Content { get; set; } = null!;
}