using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

public class GamingViewModel
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool PlayableOnly { get; set; }
    public string Sort { get; set; } = "popular";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalItems { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<Content> Games { get; set; } = new();
}

public class GameDetailViewModel
{
    public Content Game { get; set; } = null!;
    public MediaItem? Trailer { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? MyStars { get; set; }
    public bool IsBookmarked { get; set; }
    public List<Content> RelatedGames { get; set; } = new();
}
