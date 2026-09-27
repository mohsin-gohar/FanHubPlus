using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Blog archive with layout switching (list / grid / left / right sidebar) and paging.</summary>
    public class BlogListViewModel
    {
        public string Layout { get; set; } = "right";     // list | grid | left | right
        public List<BlogPost> Posts { get; set; } = new();
        public List<BlogPost> Latest { get; set; } = new();
        public List<BlogPost> Popular { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public bool ShowSidebar => Layout is "left" or "right";
    }
}
