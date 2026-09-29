using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

/// <summary>View model for the Movies &amp; TV Shows streaming library index page.</summary>
public class StreamingViewModel
{
    public string? Search      { get; set; }
    public int?    CategoryId  { get; set; }
    public string? Genre       { get; set; }
    public string? ContentType { get; set; }  // "Movie" | "Series" | "Documentary" | "Special"
    public string  Sort        { get; set; } = "popular";
    public int     Page        { get; set; } = 1;
    public int     PageSize    { get; set; } = 16;
    public int     TotalItems  { get; set; }
    public int     TotalPages  => (int)Math.Ceiling(TotalItems / (double)PageSize);

    public List<Category> Categories { get; set; } = new();
    public List<Content>  Items      { get; set; } = new();
    public List<Content>  Featured   { get; set; } = new();  // hero carousel

    /// <summary>Builds query string preserving active filters when paging.</summary>
    public string PageUrl(int page)
    {
        var q = new System.Collections.Generic.List<string> { $"page={page}" };
        if (!string.IsNullOrWhiteSpace(Search))      q.Add($"search={Uri.EscapeDataString(Search)}");
        if (CategoryId is > 0)                       q.Add($"categoryId={CategoryId}");
        if (!string.IsNullOrWhiteSpace(ContentType)) q.Add($"contentType={ContentType}");
        if (!string.IsNullOrWhiteSpace(Genre))       q.Add($"genre={Uri.EscapeDataString(Genre)}");
        q.Add($"sort={Sort}");
        return "?" + string.Join("&", q);
    }
}

/// <summary>One review card in the Reviews block of a streaming detail page.</summary>
public class ContentReviewViewModel
{
    public string   Author    { get; set; } = "FanHubPlus member";
    public string   AvatarUrl { get; set; } = "/assets/images/users/user1.jpg";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int      Stars     { get; set; }             // 1..5
    public string?  Text      { get; set; }             // the written part of the review
}

/// <summary>View model for a single streaming item detail page.</summary>
public class StreamingDetailViewModel
{
    public Content   Item          { get; set; } = null!;
    public MediaItem? Trailer      { get; set; }
    public double    AverageRating { get; set; }
    public int       RatingCount   { get; set; }
    public int?      MyStars       { get; set; }
    public bool      IsBookmarked  { get; set; }
    public List<Content> Related   { get; set; } = new();

    // ---- Reviews block (template's "Reviews" cards) ----
    public List<ContentReviewViewModel> Reviews { get; set; } = new();

    // ---- Sidebar counters: thumbs up / down buttons and the "Seen" button ----
    public int  UpVotes   { get; set; }   // ratings of 4-5 stars
    public int  DownVotes { get; set; }   // ratings of 1-2 stars
    public int  SeenCount { get; set; }   // visitors who pressed "Seen"
    public bool IsSeen    { get; set; }

    // ---- "Featured band" under the page: top-rated titles in the library ----
    public List<Content> Featured { get; set; } = new();

    /// <summary>Rating figures for the "Related Movies" cards (one grouped query, no N+1).</summary>
    public Dictionary<int, (double Avg, int Count)> RelatedRatings { get; set; } = new();

    /// <summary>Every playable row of the item (video / trailer / audio) in a stable order.</summary>
    public IEnumerable<MediaItem> WatchOptions =>
        Item.MediaItems
            .Where(m => Services.MediaUrl.IsPlayable(m))
            .OrderBy(m => m.MediaType == Models.Enums.MediaType.Video ? 0 : 1)
            .ThenBy(m => m.MediaId);

    /// <summary>145 -> "2h : 25M" (the template's runtime format).</summary>
    public static string FormatRuntime(int? minutes)
    {
        if (minutes is null or <= 0) return "—";
        var h = minutes.Value / 60;
        var m = minutes.Value % 60;
        return h > 0 ? $"{h}h : {m}M" : $"{m}M";
    }

    /// <summary>The cast column is stored as "Name:Role, Name:Role" - parsed into cards.</summary>
    public static List<(string Name, string Role)> ParseCast(string? raw)
    {
        var list = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(raw)) return list;

        foreach (var chunk in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = chunk.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts[0].Length == 0) continue;
            list.Add((parts[0], parts.Length > 1 && parts[1].Length > 0 ? parts[1] : "Actor"));
        }
        return list;
    }
}
