namespace FanHubPlus.DTOs;

public class VideoFilter
{
    public VideoType Type { get; set; } = VideoType.Movies;
    public string? Genre { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "latest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    public string GetCacheKey()
    {
        return $"{Type}_{Genre}_{Search}_{SortBy}_{Page}_{PageSize}";
    }
}

public enum VideoType
{
    Movies = 1,
    TvShows = 2,
    Live = 3,
    Videos = 4
}

public class VideoListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Duration { get; set; } = string.Empty;
    public double ImdbRating { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsTrending { get; set; }
    public bool IsTopRated { get; set; }
    public bool IsLive { get; set; }
    public int WatchingNow { get; set; }
    public string PosterTheme { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
}

public class VideoDetailDto
{
    public VideoItem Video { get; set; } = null!;
    public List<Episode> Episodes { get; set; } = new();
    public List<VideoItem> Related { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public List<Comment> Replies { get; set; } = new();
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}