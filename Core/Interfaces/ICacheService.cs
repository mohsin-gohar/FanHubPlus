using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FanHubPlus.Core.Interfaces;

/// <summary>
/// Multi-tier cache abstraction supporting L1 (Memory) and L2 (Redis) caching.
/// Implementations should handle serialization, compression, and cache invalidation.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache, or computes and caches it if not present.
    /// </summary>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache without computing.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache with optional expiration.
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a key from cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all keys matching a pattern (Redis only; MemoryCache clears all).
    /// </summary>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values at once (batch get).
    /// </summary>
    Task<Dictionary<string, T?>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple values at once (batch set).
    /// </summary>
    Task SetManyAsync<T>(
        Dictionary<string, T> items,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache key builder for consistent, typed cache keys across the application.
/// </summary>
public static class CacheKeys
{
    public static string Categories => "categories:all";
    public static string Category(int id) => $"category:{id}";
    public static string CategoryWithContents(int id) => $"category:{id}:contents";

    public static string Content(int id) => $"content:{id}";
    public static string ContentDetail(int id) => $"content:{id}:detail";
    public static string Trending(int count) => $"trending:{count}";
    public static string Trailers(int count) => $"trailers:{count}";
    public static string Explore(string? search, int? categoryId, string? type, string sort, int page)
        => $"explore:{search ?? "all"}:{categoryId ?? 0}:{type ?? "all"}:{sort}:{page}";

    public static string UserProfile(string userId) => $"user:{userId}:profile";
    public static string UserFavorites(string userId) => $"user:{userId}:favorites";
    public static string UserBookmarks(string userId, string itemType) => $"user:{userId}:bookmarks:{itemType}";

    public static string Article(int id) => $"article:{id}";
    public static string LatestArticles(int count) => $"articles:latest:{count}";
    public static string Events(int count) => $"events:upcoming:{count}";

    public static string Merchandise(int count) => $"merch:featured:{count}";
    public static string Game(int id) => $"game:{id}";
    public static string Games(string? genre, string? platform, int page)
        => $"games:{genre ?? "all"}:{platform ?? "all"}:{page}";

    public static string ChatFaqs => "chat:faqs";
    public static string ChatResponse(string queryHash) => $"chat:response:{queryHash}";

    public static string StatsTotals => "stats:totals";

    public static string InvalidateCategory(int id)
    {
        var keys = new[]
        {
            Categories,
            Category(id),
            CategoryWithContents(id),
            Trending(6),
            Trending(10),
            Trailers(6),
            Explore(null, id, null, "popular", 1)
        };
        return string.Join("|", keys);
    }

    public static string InvalidateContent(int id)
    {
        var keys = new[]
        {
            Content(id),
            ContentDetail(id),
            Trending(6),
            Trending(10),
            Trailers(6),
            $"explore:*" // Pattern for Redis
        };
        return string.Join("|", keys);
    }

    public static string InvalidateUser(string userId)
    {
        return $"user:{userId}:*";
    }
}