using FanHubPlus.Models.Enums;

namespace FanHubPlus.Services;

/// <summary>
/// Routes a catalogue item to the hub that can actually play it
/// (watch / listen / play-in-browser) instead of a generic explorer page.
/// </summary>
public static class ContentLinks
{
    public static string ControllerFor(ContentType type) => type switch
    {
        ContentType.Song or ContentType.Album => "Music",
        ContentType.Game or ContentType.PlayableGame => "Gaming",
        ContentType.Movie or ContentType.Series or ContentType.Documentary or ContentType.Special => "Streaming",
        _ => "Explore"
    };
}
