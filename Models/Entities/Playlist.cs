using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// Custom user playlist for music tracks, videos, and media items
public class Playlist
{
    public int PlaylistId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;   // FK -> ApplicationUser

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsPublic { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}
