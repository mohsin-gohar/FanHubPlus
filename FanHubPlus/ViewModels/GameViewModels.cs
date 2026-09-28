using FanHubPlus.Games.Models;

namespace FanHubPlus.ViewModels;

public class GamesIndexViewModel
{
    public List<GameListDto> Games { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public GameGenre? Genre { get; set; }
    public GamePlatform? Platform { get; set; }
    public string Sort { get; set; } = "popular";
    public IEnumerable<GameGenre> Genres { get; set; } = Array.Empty<GameGenre>();
    public IEnumerable<GamePlatform> Platforms { get; set; } = Array.Empty<GamePlatform>();
    public bool HasFilters => !string.IsNullOrWhiteSpace(Search) || Genre.HasValue || Platform.HasValue;
}

public class GameDetailsViewModel
{
    public GameDetailDto Game { get; set; } = null!;
}

public class GamePlayViewModel
{
    public GamePlayDto Game { get; set; } = null!;
}