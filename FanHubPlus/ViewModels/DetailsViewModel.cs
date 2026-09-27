using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Everything the title details page needs in one payload.</summary>
    public class DetailsViewModel
    {
        public VideoItem Title { get; set; } = new();
        public List<Episode> Episodes { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public List<Comment> Replies { get; set; } = new();
        public List<VideoItem> Related { get; set; } = new();
        public int WatchlistCount { get; set; }
        public int AverageScore { get; set; } = 86;

        public IReadOnlyList<string> CastList =>
            (Title.Cast ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        public bool HasEpisodes => Episodes.Count > 0;
    }
}
