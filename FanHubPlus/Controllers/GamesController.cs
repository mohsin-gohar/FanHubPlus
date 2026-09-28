using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Games.Models;
using FanHubPlus.Games.Services;
using FanHubPlus.Models;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Controllers;

public class GamesController : Controller
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search, GameGenre? genre, GamePlatform? platform, string sort = "popular", int page = 1)
    {
        var filter = new GameFilter
        {
            Search = search,
            Genre = genre,
            Platform = platform,
            Sort = sort,
            Page = Math.Max(1, page),
            PageSize = 20
        };

        var result = await _gameService.GetGamesAsync(filter);

        var model = new GamesIndexViewModel
        {
            Games = result.Items,
            TotalCount = result.TotalItems,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            Search = search,
            Genre = genre,
            Platform = platform,
            Sort = sort,
            Genres = Enum.GetValues<GameGenre>(),
            Platforms = Enum.GetValues<GamePlatform>()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _gameService.GetGameDetailAsync(id, User.Identity?.Name);
        if (detail == null) return NotFound();

        var model = new GameDetailsViewModel
        {
            Game = detail
        };

        return View(model);
    }

    public async Task<IActionResult> Play(int id)
    {
        var playData = await _gameService.GetPlayDataAsync(id, User.Identity?.Name);
        
        var model = new GamePlayViewModel
        {
            Game = playData
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGame(int gameId, string slotName, string saveData)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Json(new { success = false, message = "Please sign in to save your progress." });
        }

        try
        {
            var save = await _gameService.SaveGameAsync(gameId, User.Identity!.Name!, slotName, saveData);
            return Json(new { success = true, save });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save game {GameId} for user {UserId}", gameId, User.Identity?.Name);
            return Json(new { success = false, message = "Failed to save game." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadGame(int gameId, string slotName = "default")
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Json(new { success = false, message = "Please sign in to load your progress." });
        }

        try
        {
            var save = await _gameService.LoadGameAsync(gameId, User.Identity!.Name!, slotName);
            return Json(new { success = true, save });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load game {GameId} for user {UserId}", gameId, User.Identity?.Name);
            return Json(new { success = false, message = "Failed to load game." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitScore(int gameId, long score, int? level = null, string? time = null)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Json(new { success = false, message = "Please sign in to submit your score." });
        }

        try
        {
            TimeSpan? timeSpan = null;
            if (!string.IsNullOrEmpty(time) && TimeSpan.TryParse(time, out var ts))
            {
                timeSpan = ts;
            }

            await _gameService.SubmitScoreAsync(
                gameId, 
                User.Identity!.Name!, 
                User.Identity.Name!, 
                score, 
                level, 
                timeSpan);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit score for game {GameId}", gameId);
            return Json(new { success = false, message = "Failed to submit score." });
        }
    }

    public async Task<IActionResult> Leaderboard(int id)
    {
        var leaderboard = await _gameService.GetLeaderboardAsync(id, 100);
        return View(leaderboard);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordSession(int gameId, GameSessionDto session)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Json(new { success = false });
        }

        try
        {
            await _gameService.RecordPlaySessionAsync(gameId, User.Identity!.Name!, session);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record session for game {GameId}", gameId);
            return Json(new { success = false });
        }
    }
}