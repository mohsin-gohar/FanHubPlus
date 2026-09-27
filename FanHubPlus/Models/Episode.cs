namespace FanHubPlus.Models
{
    /// <summary>One episode of a series shown on the title details page.</summary>
    public class Episode
    {
        public int Id { get; set; }
        public int VideoId { get; set; }
        public int Season { get; set; } = 1;
        public int Number { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = "42m";
        public double ImdbRating { get; set; }
        public string StillTheme { get; set; } = "t1";
        public bool IsFree { get; set; }
        public DateTime AiredOn { get; set; } = DateTime.Now;

        public string Code => $"S{Season:00}E{Number:00}";
        public string AiredDisplay => AiredOn.ToString("dd MMM yyyy");
    }
}
