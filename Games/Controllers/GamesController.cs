using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FanHubPlus.Games.Models;
using FanHubPlus.Games.Services;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Games.Controllers;

/// <summary>
/// MVC Controller for browsing and playing games.
/// </summary>
[Route("games")]
public class GamesController : Controller
{
    private readonly IGameService _gameService;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Game> _games;

    public GamesController(
        IGameService gameService,
        IRepository<Category> categories,
        IRepository<Game> games)
    {
        _gameService = gameService;
        _categories = categories;
        _games = games;
    }

    /// <summary>
    /// Browse games with filters and pagination.
    /// GET /games?search=&genre=&platform=&sort=&page=
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? search,
        GameGenre? genre,
        GamePlatform? platform,
        string sort = "popular",
        int page = 1)
    {
        var filter = new GameFilter
        {
            Search = search,
            Genre = genre,
            Platform = platform,
            Sort = sort,
            Page = Math.Max(1, page),
            PageSize = 20,
            VerifiedOnly = true
        };

        var result = await _gameService.GetGamesAsync(filter);
        var categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync();
        var genres = Enum.GetValues<GameGenre>();
        var platforms = Enum.GetValues<GamePlatform>();

        var vm = new GamesIndexViewModel
        {
            Games = result,
            Filter = filter,
            Categories = categories,
            Genres = genres,
            Platforms = platforms
        };

        return View(vm);
    }

    /// <summary>
    /// Game detail page.
    /// GET /games/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;

        var game = await _gameService.GetGameDetailAsync(id, userId);
        if (game is null)
            return NotFound();

        return View(game);
    }

    /// <summary>
    /// Play game page with embedded player.
    /// GET /games/{id}/play
    /// </summary    [HttpGet("{id:int}/play")]
    [Authorize]
    public async Task<IActionResult> Play(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!;
        var playData = await _gameService.GetPlayDataAsync(id, userId);

        return View(playData);
    }

    /// <summary>
    /// Featured games partial for homepage.
    /// </summary>
    [HttpGet("featured")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> Featured(int count = 6)
    {
        var filter = new GameFilter
        {
            FeaturedOnly = true,
            VerifiedOnly = true,
            Sort = "popular",
            Page = 1,
            PageSize = count
        };

        var result = await _gameService.GetGamesAsync(filter);
        return PartialView("_FeaturedGames", result.Items);
    }

    /// <summary>
    /// AJAX endpoint for rating a game.
    /// </summary>
    [HttpPost("{id:int}/rate")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Rate(int id, int stars)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Json(new { ok = false, auth = false, message = "Please log in to rate." });

        if (stars < 1 || stars > 5)
            return Json(new { ok = false, message = "Stars must be between 1 and 5." });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var userName = User.Identity?.Name ?? "Anonymous";

        await _gameService.SubmitScoreAsync(id, userId, userName, stars * 1000); // Using score as rating proxy

        return Json(new { ok = true, message = "Thanks for rating!" });
    }

    /// <summary>
    /// AJAX endpoint for saving game progress.
    /// </summary>
    [HttpPost("{id:int}/save")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Save(int id, string slotName, string saveData)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Json(new { ok = false, auth = false, message = "Please log in to save." });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        try
        {
            var save = await _gameService.SaveGameAsync(id, userId, slotName, saveData);
            return Json(new { ok = true, save });
        }
        catch (Exception ex)
        {
            return Json(new { ok = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for loading game progress.
    /// </summary>
    [HttpGet("{id:int}/load")]
    public async Task<IActionResult> Load(int id, string slotName = "default")
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Json(new { ok = false, auth = false, message = "Please log in to load." });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var save = await _gameService.LoadGameAsync(id, userId, slotName);
        if (save is null)
            return Json(new { ok = false, message = "No save found." });

        return Json(new { ok = true, save });
    }

    /// <summary>
    /// List user's save slots for a game.
    /// </summary>
    [HttpGet("{id:int}/saves")]
    public async Task<IActionResult> ListSaves(int id)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Json(new { ok = false, auth = false });

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var saves = await _gameService.ListSavesAsync(id, userId);

        return Json(new { ok = true, saves });
    }

    /// <summary>
    /// Leaderboard for a game.
    /// </summary>
    [HttpGet("{id:int}/leaderboard")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> Leaderboard(int id, int top = 100)
    {
        var leaderboard = await _gameService.GetLeaderboardAsync(id, top);
        return View(leaderboard);
    }
}

/// <summary>
/// View model for games index page.
/// </summary>
public class GamesIndexViewModel
{
    public PagedResult<GameListDto> Games { get; set; } = new();
    public GameFilter Filter { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public IEnumerable<GameGenre> Genres { get; set; } = Enum.GetValues<GameGenre>();
    public IEnumerable<GamePlatform> Platforms { get; set; } = Enum.GetValues<GamePlatform>();
}