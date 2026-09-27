using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Backing model for every catalogue page (movies, TV shows, videos, categories).</summary>
    public class BrowseViewModel
    {
        public string Section { get; set; } = "Movies";      // Movies | TV Shows | Videos | Categories
        public List<VideoItem> Items { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<Channel> Channels { get; set; } = new();

        public string? Genre { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }                   // latest | rating | year | title
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public bool HasFilters => !string.IsNullOrWhiteSpace(Genre) || !string.IsNullOrWhiteSpace(Search);

        public static int SkipTake(int page, int pageSize) => (Math.Max(1, page) - 1) * pageSize;
    }
}
