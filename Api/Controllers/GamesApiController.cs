using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FanHubPlus.Games.Models;
using FanHubPlus.Games.Services;
using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace FanHubPlus.Api.Controllers;

/// <summary>
/// REST API for games - enables mobile apps, SPAs, and third-party integrations.
/// </summary>
[ApiController]
[Route("api/v1/games")]
[Produces("application/json")]
[SwaggerTag("Game management and play APIs")]
public class GamesApiController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesApiController(IGameService gameService)
    {
        _gameService = gameService;
    }

    /// <summary>
    /// Get paginated list of games with filtering.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "List games", Description = "Returns paginated games with optional filters")]
    [SwaggerResponse(200, "Success", typeof(PagedResult<GameListDto>))]
    [ResponseCache(Duration = 30, VaryByQueryKeys = new[] { "search", "genre", "platform", "sort", "page", "pageSize" })]
    public async Task<ActionResult<PagedResult<GameListDto>>> GetGames(
        [FromQuery] string? search = null,
        [FromQuery] GameGenre? genre = null,
        [FromQuery] GamePlatform? platform = null,
        [FromQuery] string sort = "popular",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var filter = new GameFilter
        {
            Search = search,
            Genre = genre,
            Platform = platform,
            Sort = sort,
            Page = page,
            PageSize = pageSize,
            VerifiedOnly = true
        };

        var result = await _gameService.GetGamesAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed game information.
    /// </summary>
    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get game details")]
    [SwaggerResponse(200, "Success", typeof(GameDetailDto))]
    [SwaggerResponse(404, "Game not found")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "userId" })]
    public async Task<ActionResult<GameDetailDto>> GetGame(int id, [FromQuery] string? userId = null)
    {
        var game = await _gameService.GetGameDetailAsync(id, userId);
        if (game is null)
            return NotFound(new { error = "Game not found", code = "NOT_FOUND" });

        return Ok(game);
    }

    /// <summary>
    /// Get play data for embedding/launching a game.
    /// </summary>
    [HttpGet("{id:int}/play")]
    [Authorize]
    [SwaggerOperation(Summary = "Get game play data", Description = "Returns embed URL, sandbox attributes, and cloud save info")]
    [SwaggerResponse(200, "Success", typeof(GamePlayDto))]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(404, "Game not found")]
    public async Task<ActionResult<GamePlayDto>> GetPlayData(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var playData = await _gameService.GetPlayDataAsync(id, userId);
        return Ok(playData);
    }

    /// <summary>
    /// Save game progress to cloud.
    /// </summary>
    [HttpPost("{id:int}/saves")]
    [Authorize]
    [EnableRateLimiting("write")]
    [SwaggerOperation(Summary = "Save game progress")]
    [SwaggerResponse(200, "Saved", typeof(GameSaveDto))]
    [SwaggerResponse(400, "Invalid data")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<GameSaveDto>> SaveGame(
        int id,
        [FromBody] SaveGameRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        if (string.IsNullOrWhiteSpace(request.SlotName))
            return BadRequest(new { error = "Slot name required", code = "VALIDATION_ERROR" });

        if (string.IsNullOrWhiteSpace(request.SaveData))
            return BadRequest(new { error = "Save data required", code = "VALIDATION_ERROR" });

        var save = await _gameService.SaveGameAsync(id, userId, request.SlotName, request.SaveData);
        return Ok(save);
    }

    /// <summary>
    /// Load game progress from cloud.
    /// </summary>
    [HttpGet("{id:int}/saves/{slotName}")]
    [Authorize]
    [SwaggerOperation(Summary = "Load game progress")]
    [SwaggerResponse(200, "Success", typeof(GameSaveDto))]
    [SwaggerResponse(404, "Save not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<GameSaveDto>> LoadGame(int id, string slotName = "default")
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var save = await _gameService.LoadGameAsync(id, userId, slotName);

        if (save is null)
            return NotFound(new { error = "Save not found", code = "NOT_FOUND" });

        return Ok(save);
    }

    /// <summary>
    /// List all save slots for a game.
    /// </summary>
    [HttpGet("{id:int}/saves")]
    [Authorize]
    [SwaggerOperation(Summary = "List save slots")]
    [SwaggerResponse(200, "Success", typeof(List<GameSaveDto>))]
    public async Task<ActionResult<List<GameSaveDto>>> ListSaves(int id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var saves = await _gameService.ListSavesAsync(id, userId);
        return Ok(saves);
    }

    /// <summary>
    /// Record a play session (analytics).
    /// </summary>
    [HttpPost("{id:int}/sessions")]
    [Authorize]
    [EnableRateLimiting("write")]
    [SwaggerOperation(Summary = "Record play session")]
    [SwaggerResponse(204, "Recorded")]
    public async Task<IActionResult> RecordSession(int id, [FromBody] GameSessionDto session)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        await _gameService.RecordPlaySessionAsync(id, userId, session);
        return NoContent();
    }

    /// <summary>
    /// Get game leaderboard.
    /// </summary>
    [HttpGet("{id:int}/leaderboard")]
    [SwaggerOperation(Summary = "Get leaderboard")]
    [SwaggerResponse(200, "Success", typeof(LeaderboardDto))]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<LeaderboardDto>> GetLeaderboard(int id, [FromQuery] int top = 100)
    {
        top = Math.Clamp(top, 1, 500);
        var leaderboard = await _gameService.GetLeaderboardAsync(id, top);
        return Ok(leaderboard);
    }

    /// <summary>
    /// Submit a score to the leaderboard.
    /// </summary>
    [HttpPost("{id:int}/leaderboard")]
    [Authorize]
    [EnableRateLimiting("write")]
    [SwaggerOperation(Summary = "Submit score")]
    [SwaggerResponse(204, "Submitted")]
    [SwaggerResponse(400, "Invalid score")]
    public async Task<IActionResult> SubmitScore(
        int id,
        [FromBody] SubmitScoreRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var playerName = User.Identity?.Name ?? "Anonymous";

        if (request.Score < 0)
            return BadRequest(new { error = "Score must be non-negative", code = "VALIDATION_ERROR" });

        await _gameService.SubmitScoreAsync(id, userId, playerName, request.Score, request.Level, request.Time, request.Metadata);
        return NoContent();
    }

    /// <summary>
    /// Get featured games for homepage carousel.
    /// </summary>
    [HttpGet("featured")]
    [SwaggerOperation(Summary = "Get featured games")]
    [SwaggerResponse(200, "Success", typeof(List<GameListDto>))]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<List<GameListDto>>> GetFeatured([FromQuery] int count = 6)
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
        return Ok(result.Items);
    }

    /// <summary>
    /// Get games by genre.
    /// </summary>
    [HttpGet("by-genre/{genre}")]
    [SwaggerOperation(Summary = "Get games by genre")]
    [SwaggerResponse(200, "Success", typeof(PagedResult<GameListDto>))]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<PagedResult<GameListDto>>> GetByGenre(
        GameGenre genre,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new GameFilter
        {
            Genre = genre,
            VerifiedOnly = true,
            Page = page,
            PageSize = pageSize
        };

        var result = await _gameService.GetGamesAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// Get games by platform.
    /// </summary>
    [HttpGet("by-platform/{platform}")]
    [SwaggerOperation(Summary = "Get games by platform")]
    [SwaggerResponse(200, "Success", typeof(PagedResult<GameListDto>))]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<PagedResult<GameListDto>>> GetByPlatform(
        GamePlatform platform,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new GameFilter
        {
            Platform = platform,
            VerifiedOnly = true,
            Page = page,
            PageSize = pageSize
        };

        var result = await _gameService.GetGamesAsync(filter);
        return Ok(result);
    }
}

/// <summary>
/// Request DTO for saving game.
/// </summary>
public class SaveGameRequest
{
    public string SlotName { get; set; } = "default";
    public string SaveData { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for submitting score.
/// </summary>
public class SubmitScoreRequest
{
    public long Score { get; set; }
    public int? Level { get; set; }
    public TimeSpan? Time { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}