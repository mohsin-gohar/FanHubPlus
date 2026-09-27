using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Data for the shared blog sidebar (search, categories, latest, popular, tags).</summary>
    public class BlogSidebarViewModel
    {
        public List<BlogPost> Latest { get; set; } = new();
        public List<BlogPost> Popular { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string? ActiveCategory { get; set; }
    }
}
