using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FanHubPlus.Games.Models;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using FanHubPlus.Core.Interfaces;
using FanHubPlus.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Games.Services;

/// <summary>
/// Service for managing games and game-related operations.
/// </summary>
public interface IGameService
{
    Task<Game?> GetGameAsync(int id);
    Task<GameDetailDto?> GetGameDetailAsync(int id, string? userId = null);
    Task<PagedResult<GameListDto>> GetGamesAsync(GameFilter filter);
    Task<Game> CreateGameAsync(CreateGameDto dto);
    Task<Game> UpdateGameAsync(int id, UpdateGameDto dto);
    Task DeleteGameAsync(int id);
    Task<GamePlayDto> GetPlayDataAsync(int id, string? userId = null);
    Task<GameSaveDto> SaveGameAsync(int gameId, string userId, string slotName, string saveData);
    Task<GameSaveDto?> LoadGameAsync(int gameId, string userId, string slotName = "default");
    Task<List<GameSaveDto>> ListSavesAsync(int gameId, string userId);
    Task RecordPlaySessionAsync(int gameId, string userId, GameSessionDto session);
    Task<LeaderboardDto> GetLeaderboardAsync(int gameId, int top = 100);
    Task SubmitScoreAsync(int gameId, string userId, string playerName, long score, int? level = null, TimeSpan? time = null, Dictionary<string, object>? metadata = null);
    Task IncrementPlayCountAsync(int gameId);
    Task<GameUploadDto> UploadGameFilesAsync(int gameId, IFormFile file);
    Task ProcessGameUploadAsync(int uploadId);
}

/// <summary>
/// Game filter for listing/searching games.
/// </summary>
public class GameFilter
{
    public string? Search { get; set; }
    public GameGenre? Genre { get; set; }
    public GamePlatform? Platform { get; set; }
    public GameControls? RequiredControls { get; set; }
    public bool? FeaturedOnly { get; set; }
    public bool? VerifiedOnly { get; set; } = true;
    public string Sort { get; set; } = "popular"; // popular, newest, plays, rating, alphabetical
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Paged result wrapper.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

/// <summary>
/// Lightweight game DTO for lists.
/// </summary>
public class GameListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public GameGenre Genre { get; set; }
    public GamePlatform Platform { get; set; }
    public GameControls Controls { get; set; }
    public bool SupportsMultiplayer { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsVerified { get; set; }
    public int PlayCount { get; set; }
    public int AvgSessionMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Engine { get; set; }
}

/// <summary>
/// Detailed game DTO for detail page.
/// </summary>
public class GameDetailDto : GameListDto
{
    public string? FullDescription { get; set; }
    public string? PlayUrl { get; set; }
    public string? EmbedHtml { get; set; }
    public string? LaunchCommand { get; set; }
    public int? SteamAppId { get; set; }
    public string? EpicNamespace { get; set; }
    public string? ItchIoUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? Version { get; set; }
    public string? RequiredFeatures { get; set; }
    public int CompletionRate { get; set; }
    public List<GameLeaderboardDto> TopScores { get; set; } = new();
    public List<GameSaveDto> UserSaves { get; set; } = new();
    public bool UserHasPlayed { get; set; }
    public int UserPlayCount { get; set; }
    public int UserBestScore { get; set; }
}

/// <summary>
/// DTO for game play page.
/// </summary>
public class GamePlayDto
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PlayUrl { get; set; } = string.Empty;
    public string? EmbedHtml { get; set; }
    public GamePlatform Platform { get; set; }
    public GameControls Controls { get; set; }
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public bool AllowFullscreen { get; set; } = true;
    public string? SandboxAttributes { get; set; }
    public GameSaveDto? LatestSave { get; set; }
    public bool SupportsCloudSave { get; set; }
    public string SaveDataKey { get; set; } = string.Empty;
}

/// <summary>
/// DTO for game save data.
/// </summary>
public class GameSaveDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string SaveData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for play session recording.
/// </summary>
public class GameSessionDto
{
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? DeviceInfo { get; set; }
    public string? BrowserInfo { get; set; }
    public bool Completed { get; set; }
}

/// <summary>
/// DTO for leaderboard.
/// </summary>
public class LeaderboardDto
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public List<GameLeaderboardDto> Entries { get; set; } = new();
    public GameLeaderboardDto? UserEntry { get; set; }
    public int TotalEntries { get; set; }
}

public class GameLeaderboardDto
{
    public int Rank { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public long Score { get; set; }
    public int? Level { get; set; }
    public TimeSpan? Time { get; set; }
    public DateTime AchievedAt { get; set; }
    public bool IsCurrentUser { get; set; }
}

/// <summary>
/// DTO for creating a game.
/// </summary>
public class CreateGameDto
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty!;
    public GameGenre Genre { get; set; }
    public GamePlatform Platform { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PlayUrl { get; set; }
    public string? EmbedHtml { get; set; }
    public GameControls Controls { get; set; }
    public bool SupportsMultiplayer { get; set; }
    public int MinPlayers { get; set; } = 1;
    public int MaxPlayers { get; set; } = 1;
    public string? SaveDataKey { get; set; }
    public string? Engine { get; set; }
    public string? Version { get; set; }
    public string? RequiredFeatures { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? LaunchCommand { get; set; }
    public int? SteamAppId { get; set; }
    public string? EpicNamespace { get; set; }
    public string? ItchIoUrl { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public int PopularityScore { get; set; } = 0;
    public string[]? Tags { get; set; }
}

/// <summary>
/// DTO for updating a game.
/// </summary>
public class UpdateGameDto
{
    public string? Title { get; set; }
    public GameGenre? Genre { get; set; }
    public GamePlatform? Platform { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PlayUrl { get; set; }
    public string? EmbedHtml { get; set; }
    public GameControls? Controls { get; set; }
    public bool? SupportsMultiplayer { get; set; }
    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }
    public string? SaveDataKey { get; set; }
    public string? Engine { get; set; }
    public string? Version { get; set; }
    public string? RequiredFeatures { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? LaunchCommand { get; set; }
    public int? SteamAppId { get; set; }
    public string? EpicNamespace { get; set; }
    public string? ItchIoUrl { get; set; }
    public int? PopularityScore { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsVerified { get; set; }
    public string[]? Tags { get; set; }
}

/// <summary>
/// DTO for game upload.
/// </summary>
public class GameUploadDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public GameUploadStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; }
}