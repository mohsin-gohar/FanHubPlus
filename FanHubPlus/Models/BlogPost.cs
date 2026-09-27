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

        public string Category { get; set; } = "Streaming News";
        public int ReadMinutes { get; set; } = 4;
        public string Tags { get; set; } = "";                    // comma separated
        public int Comments { get; set; }
        public string ArtGlyph { get; set; } = "ri-film-line";

        public string DateDisplay => PublishedOn.ToString("dd MMM, yyyy");
        public string ReadDisplay => $"{ReadMinutes} min read";
        public IEnumerable<string> TagList =>
            (Tags ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
    }
}
