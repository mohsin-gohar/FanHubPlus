using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Lightweight category projection for the landing page — avoids loading
// the full Contents collection when only the count is displayed.
public class CategorySummaryViewModel
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
    public int ContentCount { get; set; }
}

// Everything the landing page needs in ONE query-set (avoids N+1 queries in the view)
public class HomeViewModel
{
    public List<CategorySummaryViewModel> Categories { get; set; } = new();
    public List<Content> Trending { get; set; } = new();
    public List<Article> LatestArticles { get; set; } = new();
    public List<EventItem> UpcomingEvents { get; set; } = new();
    public List<MerchandiseItem> FeaturedMerch { get; set; } = new();

    // Trailers / videos that can be played straight on the landing page
    public List<TrailerViewModel> Trailers { get; set; } = new();

    // Hero counters
    public int TotalContents { get; set; }
    public int TotalMembers { get; set; }
    public int TotalEvents { get; set; }
}
