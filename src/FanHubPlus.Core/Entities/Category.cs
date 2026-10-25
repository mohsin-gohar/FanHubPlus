using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Core.Entities;

/// <summary>
/// One fandom category (Anime, Gaming, Movies, TV Shows, K-Pop, Comics, Manga, Cosplay).
/// </summary>
public class Category
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? IconUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserCategory> FanUsers { get; set; } = new List<UserCategory>();
    public ICollection<Content> Contents { get; set; } = new List<Content>();
}