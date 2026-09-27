namespace FanHubPlus.Models
{
    /// <summary>Viewer comment shown underneath a title on the details page.</summary>
    public class Comment
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public int? ParentId { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int Likes { get; set; }
        public DateTime PostedOn { get; set; } = DateTime.Now;

        public string Initials => string.IsNullOrWhiteSpace(Author) ? "?" : Author.Trim()[..1].ToUpperInvariant();
        public string PostedDisplay => PostedOn.ToString("dd MMM yyyy, hh tt");
    }
}
