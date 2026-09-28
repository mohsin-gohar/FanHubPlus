using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

public class MusicViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string Sort { get; set; } = "popular";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<Content> Songs { get; set; } = new();
    public List<Content> Albums { get; set; } = new();
    public List<Playlist> PublicPlaylists { get; set; } = new();
}

public class MusicDetailViewModel
{
    public Content Song { get; set; } = null!;
    public MediaItem? MainAudio { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? MyStars { get; set; }
    public bool IsBookmarked { get; set; }
    public List<Content> RelatedSongs { get; set; } = new();
    public List<Playlist> MyPlaylists { get; set; } = new();
}
