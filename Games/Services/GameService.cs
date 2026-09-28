using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FanHubPlus.Games.Models;
using FanHubPlus.Games.Services;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using FanHubPlus.Core.Interfaces;
using FanHubPlus.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace FanHubPlus.Games.Services;

/// <summary>
/// Implementation of game service with caching and optimized queries.
/// </summary>
public class GameService : IGameService
{
    private readonly IRepository<Game> _games;
    private readonly IRepository<GameLeaderboard> _leaderboards;
    private readonly IRepository<GameSave> _saves;
    private readonly IRepository<GameSession> _sessions;
    private readonly IRepository<GameUpload> _uploads;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<ContentTag> _contentTags;
    private readonly IRepository<Tag> _tags;
    private readonly IContentService _contentService;
    private readonly ICacheService _cache;
    private readonly IFileUploadService _fileUpload;
    private readonly ILogger<GameService> _logger;

    public GameService(
        IRepository<Game> games,
        IRepository<GameLeaderboard> leaderboards,
        IRepository<GameSave> saves,
        IRepository<GameSession> sessions,
        IRepository<GameUpload> uploads,
        IRepository<Category> categories,
        IRepository<ContentTag> contentTags,
        IRepository<Tag> tags,
        IContentService contentService,
        ICacheService cache,
        IFileUploadService fileUpload,
        ILogger<GameService> logger)
    {
        _games = games;
        _leaderboards = leaderboards;
        _saves = saves;
        _sessions = sessions;
        _uploads = uploads;
        _categories = categories;
        _contentTags = contentTags;
        _tags = tags;
        _contentService = contentService;
        _cache = cache;
        _fileUpload = fileUpload;
        _logger = logger;
    }

    public async Task<Game?> GetGameAsync(int id)
    {
        var cacheKey = CacheKeys.Game(id);
        return await _cache.GetOrSetAsync(cacheKey, async _ =>
        {
            return await _games.Query()
                .Include(g => g.Category)
                .Include(g => g.MediaItems)
                .Include(g => g.ContentTags).ThenInclude(ct => ct.Tag)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.ContentId == id);
        }, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5));
    }

    public async Task<GameDetailDto?> GetGameDetailAsync(int id, string? userId = null)
    {
        var cacheKey = CacheKeys.Game(id) + ":detail:" + (userId ?? "anon");
        return await _cache.GetOrSetAsync(cacheKey, async _ =>
        {
            var game = await _games.Query()
                .Include(g => g.Category)
                .Include(g => g.MediaItems)
                .Include(g => g.ContentTags).ThenInclude(ct => ct.Tag)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.ContentId == id);

            if (game is null) return null;

            var topScores = await _leaderboards.Query()
                .Where(l => l.GameId == id)
                .OrderByDescending(l => l.Score)
                .Take(10)
                .Select(l => new GameLeaderboardDto
                {
                    PlayerName = l.PlayerName,
                    Score = l.Score,
                    Level = l.Level,
                    Time = l.Time,
                    AchievedAt = l.AchievedAt
                })
                .ToListAsync();

            for (int i = 0; i < topScores.Count; i++)
            {
                topScores[i].Rank = i + 1;
            }

            var userSaves = new List<GameSaveDto>();
            GameLeaderboardDto? userEntry = null;
            bool userHasPlayed = false;
            int userPlayCount = 0;
            int userBestScore = 0;

            if (!string.IsNullOrEmpty(userId))
            {
                userSaves = await _saves.Query()
                    .Where(s => s.GameId == id && s.UserId == userId)
                    .OrderByDescending(s => s.UpdatedAt)
                    .Select(s => new GameSaveDto
                    {
                        Id = s.Id,
                        GameId = s.GameId,
                        SlotName = s.SlotName,
                        SaveData = s.SaveData,
                        Version = s.Version,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                userEntry = await _leaderboards.Query()
                    .Where(l => l.GameId == id && l.UserId == userId)
                    .OrderByDescending(l => l.Score)
                    .Select(l => new GameLeaderboardDto
                    {
                        PlayerName = l.PlayerName,
                        Score = l.Score,
                        Level = l.Level,
                        Time = l.Time,
                        AchievedAt = l.AchievedAt,
                        IsCurrentUser = true
                    })
                    .FirstOrDefaultAsync();

                var userSessions = await _sessions.Query()
                    .Where(s => s.GameId == id && s.UserId == userId)
                    .ToListAsync();

                userHasPlayed = userSessions.Count > 0;
                userPlayCount = userSessions.Count;
                userBestScore = userSessions.Any() ? 0 : 0; // Would need score tracking in sessions
            }

            return new GameDetailDto
            {
                Id = game.ContentId,
                Title = game.Title,
                Description = game.Description,
                ThumbnailUrl = game.ThumbnailUrl,
                Genre = game.Genre,
                Platform = game.Platform,
                Controls = game.Controls,
                SupportsMultiplayer = game.SupportsMultiplayer,
                IsFeatured = game.IsFeatured,
                IsVerified = game.IsVerified,
                PlayCount = game.PlayCount,
                AvgSessionMinutes = game.AvgSessionMinutes,
                ReleaseDate = game.ReleaseDate ?? DateTime.MinValue,
                Engine = game.Engine,
                FullDescription = game.Description,
                PlayUrl = game.PlayUrl,
                EmbedHtml = game.EmbedHtml,
                LaunchCommand = game.LaunchCommand,
                SteamAppId = game.SteamAppId,
                EpicNamespace = game.EpicNamespace,
                ItchIoUrl = game.ItchIoUrl,
                FileSizeBytes = game.FileSizeBytes,
                Version = game.Version,
                RequiredFeatures = game.RequiredFeatures,
                CompletionRate = game.CompletionRate,
                TopScores = topScores,
                UserSaves = userSaves,
                UserHasPlayed = userHasPlayed,
                UserPlayCount = userPlayCount,
                UserBestScore = userBestScore
            };
        }, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(2));
    }

    public async Task<PagedResult<GameListDto>> GetGamesAsync(GameFilter filter)
    {
        var cacheKey = CacheKeys.Games(filter.Genre?.ToString(), filter.Platform?.ToString(), filter.Page);
        return await _cache.GetOrSetAsync(cacheKey, async _ =>
        {
            var query = _games.Query()
                .Include(g => g.Category)
                .Include(g => g.MediaItems)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(g =>
                    g.Title.ToLower().Contains(search) ||
                    (g.Description != null && g.Description.ToLower().Contains(search)) ||
                    (g.Genre != null && g.Genre.ToString().ToLower().Contains(search)));
            }

            if (filter.Genre.HasValue)
                query = query.Where(g => g.Genre == filter.Genre.Value);

            if (filter.Platform.HasValue)
                query = query.Where(g => g.Platform == filter.Platform.Value);

            if (filter.RequiredControls.HasValue)
                query = query.Where(g => (g.Controls & filter.RequiredControls.Value) == filter.RequiredControls.Value);

            if (filter.FeaturedOnly == true)
                query = query.Where(g => g.IsFeatured);

            if (filter.VerifiedOnly == true)
                query = query.Where(g => g.IsVerified);

            query = filter.Sort switch
            {
                "newest" => query.OrderByDescending(g => g.CreatedAt),
                "plays" => query.OrderByDescending(g => g.PlayCount),
                "rating" => query.OrderByDescending(g => g.PopularityScore),
                "alphabetical" => query.OrderBy(g => g.Title),
                _ => query.OrderByDescending(g => g.PopularityScore).ThenByDescending(g => g.PlayCount)
            };

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(g => new GameListDto
                {
                    Id = g.ContentId,
                    Title = g.Title,
                    Description = g.Description,
                    ThumbnailUrl = g.ThumbnailUrl,
                    Genre = g.Genre,
                    Platform = g.Platform,
                    Controls = g.Controls,
                    SupportsMultiplayer = g.SupportsMultiplayer,
                    IsFeatured = g.IsFeatured,
                    IsVerified = g.IsVerified,
                    PlayCount = g.PlayCount,
                    AvgSessionMinutes = g.AvgSessionMinutes,
                    ReleaseDate = g.ReleaseDate ?? DateTime.MinValue,
                    Engine = g.Engine
                })
                .ToListAsync();

            return new PagedResult<GameListDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(1));
    }

    public async Task<Game> CreateGameAsync(CreateGameDto dto)
    {
        var category = await _categories.Query().FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);
        if (category is null)
            throw new NotFoundException("Category", dto.CategoryId);

        var game = new Game
        {
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Genre = dto.Genre,
            Platform = dto.Platform,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumbnailUrl,
            PlayUrl = dto.PlayUrl,
            EmbedHtml = dto.EmbedHtml,
            Controls = dto.Controls,
            SupportsMultiplayer = dto.SupportsMultiplayer,
            MinPlayers = dto.MinPlayers,
            MaxPlayers = dto.MaxPlayers,
            SaveDataKey = dto.SaveDataKey ?? $"game_{Guid.NewGuid():N}",
            Engine = dto.Engine,
            Version = dto.Version,
            RequiredFeatures = dto.RequiredFeatures,
            FileSizeBytes = dto.FileSizeBytes,
            LaunchCommand = dto.LaunchCommand,
            SteamAppId = dto.SteamAppId,
            EpicNamespace = dto.EpicNamespace,
            ItchIoUrl = dto.ItchIoUrl,
            ReleaseDate = dto.ReleaseDate,
            PopularityScore = dto.PopularityScore,
            IsFeatured = false,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _games.Add(game);
        await _games.SaveChangesAsync();

        if (dto.Tags?.Length > 0)
        {
            await _contentService.SyncTagsAsync(game, string.Join(",", dto.Tags));
        }

        await InvalidateGameCaches(game.ContentId);
        _logger.LogInformation("Created game {GameId}: {Title}", game.ContentId, game.Title);

        return game;
    }

    public async Task<Game> UpdateGameAsync(int id, UpdateGameDto dto)
    {
        var game = await _games.Query().FirstOrDefaultAsync(g => g.ContentId == id);
        if (game is null)
            throw new NotFoundException("Game", id);

        if (dto.Title != null) game.Title = dto.Title;
        if (dto.Genre.HasValue) game.Genre = dto.Genre.Value;
        if (dto.Platform.HasValue) game.Platform = dto.Platform.Value;
        if (dto.Description != null) game.Description = dto.Description;
        if (dto.ThumbnailUrl != null) game.ThumbnailUrl = dto.ThumbnailUrl;
        if (dto.PlayUrl != null) game.PlayUrl = dto.PlayUrl;
        if (dto.EmbedHtml != null) game.EmbedHtml = dto.EmbedHtml;
        if (dto.Controls.HasValue) game.Controls = dto.Controls.Value;
        if (dto.SupportsMultiplayer.HasValue) game.SupportsMultiplayer = dto.SupportsMultiplayer.Value;
        if (dto.MinPlayers.HasValue) game.MinPlayers = dto.MinPlayers.Value;
        if (dto.MaxPlayers.HasValue) game.MaxPlayers = dto.MaxPlayers.Value;
        if (dto.SaveDataKey != null) game.SaveDataKey = dto.SaveDataKey;
        if (dto.Engine != null) game.Engine = dto.Engine;
        if (dto.Version != null) game.Version = dto.Version;
        if (dto.RequiredFeatures != null) game.RequiredFeatures = dto.RequiredFeatures;
        if (dto.FileSizeBytes.HasValue) game.FileSizeBytes = dto.FileSizeBytes.Value;
        if (dto.LaunchCommand != null) game.LaunchCommand = dto.LaunchCommand;
        if (dto.SteamAppId.HasValue) game.SteamAppId = dto.SteamAppId;
        if (dto.EpicNamespace != null) game.EpicNamespace = dto.EpicNamespace;
        if (dto.ItchIoUrl != null) game.ItchIoUrl = dto.ItchIoUrl;
        if (dto.PopularityScore.HasValue) game.PopularityScore = dto.PopularityScore.Value;
        if (dto.IsFeatured.HasValue) game.IsFeatured = dto.IsFeatured.Value;
        if (dto.IsVerified.HasValue) game.IsVerified = dto.IsVerified.Value;

        _games.Update(game);
        await _games.SaveChangesAsync();

        if (dto.Tags?.Length > 0)
        {
            await _contentService.SyncTagsAsync(game, string.Join(",", dto.Tags));
        }

        await InvalidateGameCaches(id);
        _logger.LogInformation("Updated game {GameId}", id);

        return game;
    }

    public async Task DeleteGameAsync(int id)
    {
        var game = await _games.Query().FirstOrDefaultAsync(g => g.ContentId == id);
        if (game is null)
            throw new NotFoundException("Game", id);

        _games.Remove(game);
        await _games.SaveChangesAsync();

        await InvalidateGameCaches(id);
        _logger.LogInformation("Deleted game {GameId}", id);
    }

    public async Task<GamePlayDto> GetPlayDataAsync(int id, string? userId = null)
    {
        var game = await GetGameAsync(id);
        if (game is null)
            throw new NotFoundException("Game", id);

        var playUrl = GetPlayUrl(game);
        var sandboxAttrs = BuildSandboxAttributes(game);

        GameSaveDto? latestSave = null;
        if (!string.IsNullOrEmpty(userId))
        {
            latestSave = await LoadGameAsync(id, userId, "default");
        }

        return new GamePlayDto
        {
            GameId = game.ContentId,
            Title = game.Title,
            PlayUrl = playUrl,
            EmbedHtml = game.EmbedHtml,
            Platform = game.Platform,
            Controls = game.Controls,
            Width = 1280,
            Height = 720,
            AllowFullscreen = true,
            SandboxAttributes = sandboxAttrs,
            LatestSave = latestSave,
            SupportsCloudSave = !string.IsNullOrEmpty(game.SaveDataKey),
            SaveDataKey = game.SaveDataKey ?? $"game_{game.ContentId}"
        };
    }

    public async Task<GameSaveDto> SaveGameAsync(int gameId, string userId, string slotName, string saveData)
    {
        var existing = await _saves.Query()
            .FirstOrDefaultAsync(s => s.GameId == gameId && s.UserId == userId && s.SlotName == slotName);

        if (existing is null)
        {
            var save = new GameSave
            {
                GameId = gameId,
                UserId = userId,
                SlotName = slotName,
                SaveData = saveData,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _saves.AddAsync(save);
            await _saves.SaveChangesAsync();

            return new GameSaveDto
            {
                Id = save.Id,
                GameId = save.GameId,
                SlotName = save.SlotName,
                SaveData = save.SaveData,
                Version = save.Version,
                CreatedAt = save.CreatedAt,
                UpdatedAt = save.UpdatedAt
            };
        }
        else
        {
            existing.SaveData = saveData;
            existing.Version++;
            existing.UpdatedAt = DateTime.UtcNow;
            _saves.Update(existing);
            await _saves.SaveChangesAsync();

            return new GameSaveDto
            {
                Id = existing.Id,
                GameId = existing.GameId,
                SlotName = existing.SlotName,
                SaveData = existing.SaveData,
                Version = existing.Version,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
        }
    }

    public async Task<GameSaveDto?> LoadGameAsync(int gameId, string userId, string slotName = "default")
    {
        var save = await _saves.Query()
            .FirstOrDefaultAsync(s => s.GameId == gameId && s.UserId == userId && s.SlotName == slotName);

        if (save is null) return null;

        return new GameSaveDto
        {
            Id = save.Id,
            GameId = save.GameId,
            SlotName = save.SlotName,
            SaveData = save.SaveData,
            Version = save.Version,
            CreatedAt = save.CreatedAt,
            UpdatedAt = save.UpdatedAt
        };
    }

    public async Task<List<GameSaveDto>> ListSavesAsync(int gameId, string userId)
    {
        return await _saves.Query()
            .Where(s => s.GameId == gameId && s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .Select(s => new GameSaveDto
            {
                Id = s.Id,
                GameId = s.GameId,
                SlotName = s.SlotName,
                SaveData = s.SaveData,
                Version = s.Version,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task RecordPlaySessionAsync(int gameId, string userId, GameSessionDto session)
    {
        var gameSession = new GameSession
        {
            GameId = gameId,
            UserId = userId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            DurationMinutes = session.DurationMinutes,
            DeviceInfo = session.DeviceInfo,
            BrowserInfo = session.BrowserInfo,
            Completed = session.Completed
        };

        await _sessions.AddAsync(gameSession);
        await _sessions.SaveChangesAsync();

        // Increment play count on game
        await IncrementPlayCountAsync(gameId);
    }

    public async Task IncrementPlayCountAsync(int gameId)
    {
        var game = await _games.Query().FirstOrDefaultAsync(g => g.ContentId == gameId);
        if (game is not null)
        {
            game.PlayCount++;
            _games.Update(game);
            await _games.SaveChangesAsync();
            await InvalidateGameCaches(gameId);
        }
    }

    public async Task<LeaderboardDto> GetLeaderboardAsync(int gameId, int top = 100)
    {
        var game = await GetGameAsync(gameId);
        if (game is null)
            throw new NotFoundException("Game", gameId);

        var entries = await _leaderboards.Query()
            .Where(l => l.GameId == gameId)
            .OrderByDescending(l => l.Score)
            .Take(top)
            .Select(l => new GameLeaderboardDto
            {
                PlayerName = l.PlayerName,
                Score = l.Score,
                Level = l.Level,
                Time = l.Time,
                AchievedAt = l.AchievedAt
            })
            .ToListAsync();

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }

        return new LeaderboardDto
        {
            GameId = gameId,
            GameTitle = game.Title,
            Entries = entries,
            TotalEntries = await _leaderboards.Query().CountAsync(l => l.GameId == gameId)
        };
    }

    public async Task SubmitScoreAsync(int gameId, string userId, string playerName, long score, int? level = null, TimeSpan? time = null, Dictionary<string, object>? metadata = null)
    {
        var existing = await _leaderboards.Query()
            .FirstOrDefaultAsync(l => l.GameId == gameId && l.UserId == userId);

        if (existing is null || score > existing.Score)
        {
            if (existing is not null)
            {
                _leaderboards.Remove(existing);
            }

            var entry = new GameLeaderboard
            {
                GameId = gameId,
                UserId = userId,
                PlayerName = playerName,
                Score = score,
                Level = level,
                Time = time,
                Metadata = metadata,
                AchievedAt = DateTime.UtcNow
            };

            await _leaderboards.AddAsync(entry);
            await _leaderboards.SaveChangesAsync();

            await InvalidateGameCaches(gameId);
        }
    }

    public async Task<GameUploadDto> UploadGameFilesAsync(int gameId, IFormFile file)
    {
        var game = await GetGameAsync(gameId);
        if (game is null)
            throw new NotFoundException("Game", gameId);

        var upload = new GameUpload
        {
            GameId = gameId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            StoragePath = $"games/{gameId}/{Guid.NewGuid():N}_{file.FileName}",
            Status = GameUploadStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        await _uploads.AddAsync(upload);
        await _uploads.SaveChangesAsync();

        // Save the file through the shared upload service (validates + writes
        // it under wwwroot/uploads and returns the public web path).
        try
        {
            var storedPath = await _fileUpload.SaveImageAsync(file, $"games/{gameId}");
            if (!string.IsNullOrWhiteSpace(storedPath))
            {
                upload.StoragePath = storedPath;
            }

            upload.Status = GameUploadStatus.Completed;
            upload.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            upload.Status = GameUploadStatus.Failed;
            upload.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to save game upload {UploadId}", upload.Id);
        }

        await _uploads.SaveChangesAsync();

        return new GameUploadDto
        {
            Id = upload.Id,
            GameId = upload.GameId,
            FileName = upload.FileName,
            FileSize = upload.FileSize,
            Status = upload.Status,
            ErrorMessage = upload.ErrorMessage,
            UploadedAt = upload.UploadedAt
        };
    }

    public async Task ProcessGameUploadAsync(int uploadId)
    {
        var upload = await _uploads.Query().FirstOrDefaultAsync(u => u.Id == uploadId);
        if (upload is null)
            throw new NotFoundException("GameUpload", uploadId);

        upload.Status = GameUploadStatus.Processing;
        await _uploads.SaveChangesAsync();

        try
        {
            // Extract and validate game files
            // This would involve unzipping, validating index.html, etc.
            // For now, just mark as completed
            upload.Status = GameUploadStatus.Completed;
            upload.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            upload.Status = GameUploadStatus.Failed;
            upload.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to process game upload {UploadId}", uploadId);
        }

        await _uploads.SaveChangesAsync();
    }

    private string GetPlayUrl(Game game)
    {
        return game.Platform switch
        {
            GamePlatform.Browser => game.PlayUrl ?? $"/games/{game.ContentId}/index.html",
            GamePlatform.ItchIo => game.ItchIoUrl ?? string.Empty,
            GamePlatform.Steam => game.SteamAppId.HasValue ? $"steam://run/{game.SteamAppId}" : string.Empty,
            GamePlatform.Epic => !string.IsNullOrEmpty(game.EpicNamespace) ? $"com.epicgames.launcher://apps/{game.EpicNamespace}?action=launch&silent=true" : string.Empty,
            GamePlatform.GOG => game.LaunchCommand ?? string.Empty,
            GamePlatform.Cloud => game.PlayUrl ?? string.Empty,
            _ => game.PlayUrl ?? string.Empty
        };
    }

    private string BuildSandboxAttributes(Game game)
    {
        var attrs = new List<string>
        {
            "allow-scripts",
            "allow-same-origin",
            "allow-forms",
            "allow-pointer-lock",
            "allow-fullscreen"
        };

        if (game.Controls.HasFlag(GameControls.Gamepad))
            attrs.Add("allow-gamepad");

        if (game.Platform == GamePlatform.ItchIo || game.Platform == GamePlatform.Browser)
        {
            attrs.Add("allow-popups");
            attrs.Add("allow-popups-to-escape-sandbox");
        }

        return string.Join(" ", attrs);
    }

    private async Task InvalidateGameCaches(int gameId)
    {
        await _cache.RemoveAsync(CacheKeys.Game(gameId));
        await _cache.RemoveAsync(CacheKeys.Game(gameId) + ":detail:*");
        await _cache.RemoveByPatternAsync(CacheKeys.Games("*", "*", 1));
        await _cache.RemoveByPatternAsync("games:*");
    }
}