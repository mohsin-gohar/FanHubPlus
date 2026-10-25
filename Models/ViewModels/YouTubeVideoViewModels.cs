namespace FanHubPlus.Models.ViewModels;

/// <summary>A single YouTube video item for display in the gallery.</summary>
public class YouTubeVideoItem
{
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Views { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>YouTube embed URL for this video.</summary>
    public string EmbedUrl => $"https://www.youtube.com/embed/{VideoId}?rel=0";

    /// <summary>Standard YouTube thumbnail URL.</summary>
    public string Thumbnail => ThumbnailUrl;
}

/// <summary>View model for the YouTube video gallery index page.</summary>
public class YouTubeVideoGalleryViewModel
{
    public List<YouTubeVideoItem> Videos { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}

/// <summary>View model for the YouTube video watch page.</summary>
public class YouTubeVideoWatchViewModel
{
    public YouTubeVideoItem Video { get; set; } = new();
    public List<YouTubeVideoItem> RelatedVideos { get; set; } = new();
}