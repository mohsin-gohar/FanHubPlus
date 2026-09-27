using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    /// <summary>Single article page: post body, author card, share rail, related reading.</summary>
    public class BlogDetailsViewModel
    {
        public BlogPost Post { get; set; } = new();
        public List<BlogPost> Related { get; set; } = new();
        public List<BlogPost> Latest { get; set; } = new();
        public List<BlogPost> Popular { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public string Sidebar { get; set; } = "right";     // left | right | none
    }
}
