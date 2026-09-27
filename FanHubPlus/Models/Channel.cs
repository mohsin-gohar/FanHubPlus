namespace FanHubPlus.Models
{
    /// <summary>A linear / on-demand channel block shown on the channel list page.</summary>
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;      // Sports | News | Entertainment | Kids | Movies
        public string Number { get; set; } = "101";
        public string Description { get; set; } = string.Empty;
        public string LogoTheme { get; set; } = "t1";
        public int Viewers { get; set; }
        public bool IsLive { get; set; } = true;
        public string Language { get; set; } = "English";
        public string NowPlaying { get; set; } = string.Empty;

        public string ViewerDisplay => Viewers >= 1000 ? $"{Viewers / 1000}K" : Viewers.ToString();
    }
}
