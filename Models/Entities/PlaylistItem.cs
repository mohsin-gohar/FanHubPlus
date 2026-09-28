using System.ComponentModel.DataAnnotations;

namespace FanHubPlus.Models.Entities;

// Item inside a user playlist
public class PlaylistItem
{
    public int PlaylistItemId { get; set; }

    public int PlaylistId { get; set; }                  // FK -> Playlist

    public int ContentId { get; set; }                   // FK -> Content

    public int SortOrder { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public Content Content { get; set; } = null!;
}
