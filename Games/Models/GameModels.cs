using System.ComponentModel.DataAnnotations;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;

namespace FanHubPlus.Games.Models;

/// <summary>
/// Represents a playable game in the platform.
/// Extends Content with game-specific properties.
/// </summary>
public class Game : Content
{
    public Game()
    {
        Type = ContentType.Game;
    }

    /// <summary>Platform the game runs on (Browser, Steam, Epic, etc.)</summary>
    public GamePlatform Platform { get; set; } = GamePlatform.Browser;

    /// <summary>Game genre classification</summary>
    public GameGenre Genre { get; set; } = GameGenre.Action;

    /// <summary>Direct play URL for browser games (iframe src)</summary>
    [StringLength(500)]
    public string? PlayUrl { get; set; }

    /// <summary>Pre-built embed HTML for complex integrations</summary>
    public string? EmbedHtml { get; set; }

    /// <summary>Supported input methods</summary>
    public GameControls Controls { get; set; } = GameControls.Keyboard | GameControls.Mouse;

    /// <summary>Whether the game supports multiplayer</summary>
    public bool SupportsMultiplayer { get; set; }

    /// <summary>Minimum players for multiplayer</summary>
    public int MinPlayers { get; set; } = 1;

    /// <summary>Maximum players for multiplayer</summary>
    public int MaxPlayers { get; set; } = 1;

    /// <summary>Key for cloud save data (IndexedDB + sync)</summary>
    [StringLength(100)]
    public string? SaveDataKey { get; set; }

    /// <summary>Game engine/technology (Unity, Godot, Phaser, Custom, WASM)</summary>
    [StringLength(50)]
    public string? Engine { get; set; }

    /// <summary>Version of the game</summary>
    [StringLength(20)]
    public string? Version { get; set; }

    /// <summary>Minimum required browser features</summary>
    [StringLength(500)]
    public string? RequiredFeatures { get; set; }

    /// <summary>Game file size in bytes (for downloaded games)</summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>Launch command for native games (Steam, Epic, etc.)</summary>
    [StringLength(200)]
    public string? LaunchCommand { get; set; }

    /// <summary>Steam App ID (if on Steam)</summary>
    public int? SteamAppId { get; set; }

    /// <summary>Epic Games namespace (if on Epic)</summary>
    [StringLength(50)]
    public string? EpicNamespace { get; set; }

    /// <summary>Itch.io game URL (for embed)</summary>
    [StringLength(500)]
    public string? ItchIoUrl { get; set; }

    /// <summary>Whether the game is featured on the homepage</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Whether the game has been verified/approved by admins</summary>
    public bool IsVerified { get; set; } = true;

    /// <summary>Average play session duration in minutes</summary>
    public int AvgSessionMinutes { get; set; }

    /// <summary>Total play count</summary>
    public int PlayCount { get; set; }

    /// <summary>Completion rate (0-100)</summary>
    public int CompletionRate { get; set; }

    // Navigation properties
    public ICollection<GameLeaderboard> Leaderboards { get; set; } = new List<GameLeaderboard>();
    public ICollection<GameSave> CloudSaves { get; set; } = new List<GameSave>();
    public ICollection<GameSession> Sessions { get; set; } = new List<GameSession>();
}

/// <summary>
/// Platform where the game can be played.
/// </summary>
public enum GamePlatform
{
    Browser = 1,      // HTML5/WASM game played in-browser
    Steam = 2,        // Steam game (launches via steam:// protocol)
    Epic = 3,         // Epic Games Store
    GOG = 4,          // GOG Galaxy
    ItchIo = 5,       // Itch.io embed
    Custom = 6,       // Custom launcher/executable
    Cloud = 7         // Cloud gaming (GeForce Now, Xbox Cloud)
}

/// <summary>
/// Game genre classification.
/// </summary>
public enum GameGenre
{
    Action = 1,
    Adventure = 2,
    RPG = 3,
    Strategy = 4,
    Puzzle = 5,
    Sports = 6,
    Racing = 7,
    Simulation = 8,
    Multiplayer = 9,
    Horror = 10,
    Platformer = 11,
    Shooter = 12,
    Fighting = 13,
    Rhythm = 14,
    Card = 15,
    Board = 16,
    Educational = 17,
    Sandbox = 18,
    Roguelike = 19,
    Metroidvania = 20,
    VisualNovel = 21,
    Idle = 22
}

/// <summary>
/// Supported input controls (flags).
/// </summary>
[Flags]
public enum GameControls
{
    None = 0,
    Keyboard = 1,
    Mouse = 2,
    Touch = 4,
    Gamepad = 8,
    Motion = 16,
    VR = 32
}

/// <summary>
/// Leaderboard entry for a game.
/// </summary>
public class GameLeaderboard
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string PlayerName { get; set; } = string.Empty;

    public long Score { get; set; }

    public int? Level { get; set; }

    public TimeSpan? Time { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Game Game { get; set; } = null!;
}

/// <summary>
/// Cloud save data for a game.
/// </summary>
public class GameSave
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string SlotName { get; set; } = "default";

    public string SaveData { get; set; } = string.Empty; // JSON serialized

    public int Version { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Game Game { get; set; } = null!;
}

/// <summary>
/// Play session tracking for analytics.
/// </summary>
public class GameSession
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? DeviceInfo { get; set; }

    public string? BrowserInfo { get; set; }

    public bool Completed { get; set; }

    // Navigation
    public Game Game { get; set; } = null!;
}

/// <summary>
/// Game upload package for admin game deployment.
/// </summary>
public class GameUpload
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public string? Checksum { get; set; }

    public GameUploadStatus Status { get; set; } = GameUploadStatus.Pending;

    public string? ErrorMessage { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    // Navigation
    public Game Game { get; set; } = null!;
}

public enum GameUploadStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}