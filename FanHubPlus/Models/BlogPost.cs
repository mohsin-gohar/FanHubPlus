namespace FanHubPlus.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = "Admin";
        public string PosterTheme { get; set; } = "t3";
        public DateTime PublishedOn { get; set; } = DateTime.Now;

        public string DateDisplay => PublishedOn.ToString("dd MMM, yyyy");
    }
}
