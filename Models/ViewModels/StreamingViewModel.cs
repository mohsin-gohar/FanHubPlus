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
}
