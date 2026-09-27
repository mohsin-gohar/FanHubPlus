namespace FanHubPlus.Models
{
    public class VideoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;      // Category name
        public int Year { get; set; }
        public string Duration { get; set; } = "1h 30m";
        public double ImdbRating { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsTrending { get; set; }
        public bool IsTopRated { get; set; }
        public bool IsLive { get; set; }
        public int WatchingNow { get; set; }
        public string PosterTheme { get; set; } = "t1";        // gradient theme for poster art
        public DateTime ReleaseDate { get; set; } = DateTime.Now;

        // Detail-page metadata
        public string Tagline { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Cast { get; set; } = string.Empty;         // comma separated
        public string Quality { get; set; } = "4K UHD";
        public string AgeRating { get; set; } = "16+";
        public string Languages { get; set; } = "English, Hindi";
        public string AudioTracks { get; set; } = "English, Hindi, Tamil";
        public string Subtitles { get; set; } = "English, Hindi, Arabic";
        public string MaturityNote { get; set; } = "Mature themes, mild violence and flashing lights.";
        public int TotalSeasons { get; set; }
        public int TotalEpisodes { get; set; }
        public bool IsSeries { get; set; }

        public string RatingDisplay => ImdbRating > 0 ? $"{ImdbRating:0.0} /10 IMDb" : "";
        public string WatchingDisplay =>
            WatchingNow >= 1000 ? $"{WatchingNow / 1000}K watching now" : $"{WatchingNow} watching now";
        public string YearDisplay => Year > 0 ? Year.ToString() : "TBA";
    }
}

