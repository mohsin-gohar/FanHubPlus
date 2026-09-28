using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FanHubPlus.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FanHubPlus.Infrastructure.Caching;

/// <summary>
/// Configuration options for the multi-tier cache.
/// </summary>
public class MultiTierCacheOptions
{
    public const string SectionName = "Cache:MultiTier";

    /// <summary>Enable L2 (Redis) cache. Default: true</summary>
    public bool EnableDistributedCache { get; set; } = true;

    /// <summary>Default absolute expiration for L1 cache. Default: 5 minutes</summary>
    public TimeSpan DefaultL1AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Default sliding expiration for L1 cache. Default: 2 minutes</summary>
    public TimeSpan DefaultL1SlidingExpiration { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Default absolute expiration for L2 cache. Default: 30 minutes</summary>
    public TimeSpan DefaultL2AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>Default sliding expiration for L2 cache. Default: 10 minutes</summary>
    public TimeSpan DefaultL2SlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Maximum size of L1 cache in bytes. Default: 100MB</summary>
    public long L1CacheSizeLimit { get; set; } = 100 * 1024 * 1024;

    /// <summary>Compress values larger than this threshold. Default: 1KB</summary>
    public int CompressionThreshold { get; set; } = 1024;
}

/// <summary>
/// Multi-tier cache implementation with L1 (MemoryCache) and L2 (Redis) layers.
/// Reads check L1 first, then L2, then compute. Writes go to both layers.
/// </summary>
public class MultiTierCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<MultiTierCacheService> _logger;
    private readonly MultiTierCacheOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<string, byte[]> _localCache = new();

    public MultiTierCacheService(
        IMemoryCache memoryCache,
        IDistributedCache? distributedCache,
        IOptions<MultiTierCacheOptions> options,
        ILogger<MultiTierCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _options = options.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        // Try L1
        if (_memoryCache.TryGetValue(key, out T? l1Value))
        {
            _logger.LogDebug("Cache HIT (L1): {Key}", key);
            return l1Value;
        }

        // Try L2
        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            var l2Value = await GetFromDistributedAsync<T>(key, cancellationToken);
            if (l2Value is not null)
            {
                _logger.LogDebug("Cache HIT (L2): {Key}", key);
                // Promote to L1
                SetInMemory(key, l2Value, absoluteExpiration, slidingExpiration);
                return l2Value;
            }
        }

        // Cache miss - compute
        _logger.LogDebug("Cache MISS: {Key}", key);
        var value = await factory(cancellationToken);

        if (value is not null)
        {
            await SetAsync(key, value, absoluteExpiration, slidingExpiration, cancellationToken);
        }

        return value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? l1Value))
        {
            _logger.LogDebug("Cache HIT (L1): {Key}", key);
            return l1Value;
        }

        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            var l2Value = await GetFromDistributedAsync<T>(key, cancellationToken);
            if (l2Value is not null)
            {
                _logger.LogDebug("Cache HIT (L2): {Key}", key);
                SetInMemory(key, l2Value, null, null);
                return l2Value;
            }
        }

        _logger.LogDebug("Cache MISS: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        // L1
        SetInMemory(key, value, absoluteExpiration, slidingExpiration);

        // L2
        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            await SetInDistributedAsync(key, value, absoluteExpiration, slidingExpiration, cancellationToken);
        }

        _logger.LogDebug("Cache SET: {Key}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _localCache.TryRemove(key, out _);

        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }

        _logger.LogDebug("Cache REMOVE: {Key}", key);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // MemoryCache doesn't support pattern removal - clear all as fallback
        if (_memoryCache is MemoryCache mc)
        {
            mc.Clear();
        }
        _localCache.Clear();

        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            // Note: Redis pattern removal requires SCAN + DEL which isn't in IDistributedCache
            // This would need a custom Redis implementation. For now, log warning.
            _logger.LogWarning("Pattern removal not supported on IDistributedCache. Pattern: {Pattern}", pattern);
        }

        _logger.LogDebug("Cache REMOVE PATTERN: {Pattern}", pattern);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out _))
            return true;

        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            var bytes = await _distributedCache.GetAsync(key, cancellationToken);
            return bytes is not null && bytes.Length > 0;
        }

        return false;
    }

    public async Task<Dictionary<string, T?>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default)
    {
        var keyList = keys.ToList();
        var result = new Dictionary<string, T?>();

        // Check L1 for all keys
        var missingKeys = new List<string>();
        foreach (var key in keyList)
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                result[key] = value;
            }
            else
            {
                missingKeys.Add(key);
            }
        }

        // Check L2 for missing keys
        if (_options.EnableDistributedCache && _distributedCache is not null && missingKeys.Count > 0)
        {
            // IDistributedCache doesn't support batch get, so we do parallel gets
            var tasks = missingKeys.Select(async key =>
            {
                var value = await GetFromDistributedAsync<T>(key, cancellationToken);
                if (value is not null)
                {
                    SetInMemory(key, value, null, null);
                    return (key, (T?)value);
                }
                return (key, default(T?));
            });

            var results = await Task.WhenAll(tasks);
            foreach (var (key, value) in results)
            {
                result[key] = value;
            }
        }

        // Fill in defaults for still-missing keys
        foreach (var key in missingKeys)
        {
            if (!result.ContainsKey(key))
                result[key] = default;
        }

        return result;
    }

    public async Task SetManyAsync<T>(
        Dictionary<string, T> items,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var (key, value) in items)
        {
            SetInMemory(key, value, absoluteExpiration, slidingExpiration);
        }

        if (_options.EnableDistributedCache && _distributedCache is not null)
        {
            var tasks = items.Select(kvp =>
                SetInDistributedAsync(kvp.Key, kvp.Value, absoluteExpiration, slidingExpiration, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }

    private void SetInMemory<T>(string key, T value, TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.DefaultL1AbsoluteExpiration,
            SlidingExpiration = slidingExpiration ?? _options.DefaultL1SlidingExpiration,
            Size = EstimateSize(value)
        };

        _memoryCache.Set(key, value, options);
    }

    private async Task<T?> GetFromDistributedAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _distributedCache!.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(bytes, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading from distributed cache: {Key}", key);
            return default;
        }
    }

    private async Task SetInDistributedAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration,
        TimeSpan? slidingExpiration,
        CancellationToken cancellationToken)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _options.DefaultL2AbsoluteExpiration,
                SlidingExpiration = slidingExpiration ?? _options.DefaultL2SlidingExpiration
            };

            await _distributedCache!.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing to distributed cache: {Key}", key);
        }
    }

    private long EstimateSize<T>(T value)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
            return bytes.Length;
        }
        catch
        {
            return 1024; // Default 1KB estimate
        }
    }
}

/// <summary>
/// Simple in-memory cache implementation for development/testing when Redis is unavailable.
/// </summary>
public class MemoryOnlyCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryOnlyCacheService> _logger;
    private readonly TimeSpan _defaultExpiration;

    public MemoryOnlyCacheService(
        IMemoryCache memoryCache,
        ILogger<MemoryOnlyCacheService> logger,
        IOptions<MultiTierCacheOptions>? options = null)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _defaultExpiration = options?.Value.DefaultL1AbsoluteExpiration ?? TimeSpan.FromMinutes(5);
    }

    public Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T?>> factory,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache MISS: {Key}", key);
        return factory(cancellationToken).ContinueWith(t =>
        {
            if (t.Result is not null)
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
                    SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(2)
                };
                _memoryCache.Set(key, t.Result, options);
            }
            return t.Result;
        }, cancellationToken);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
            SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(2)
        };
        _memoryCache.Set(key, value, options);
        _logger.LogDebug("Cache SET: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Cache REMOVE: {Key}", key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (_memoryCache is MemoryCache mc)
        {
            mc.Clear();
        }
        _logger.LogDebug("Cache CLEAR ALL (pattern not supported): {Pattern}", pattern);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public Task<Dictionary<string, T?>> GetManyAsync<T>(
        IEnumerable<string> keys,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            _memoryCache.TryGetValue(key, out T? value);
            result[key] = value;
        }
        return Task.FromResult(result);
    }

    public Task SetManyAsync<T>(
        Dictionary<string, T> items,
        TimeSpan? absoluteExpiration = null,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var (key, value) in items)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? _defaultExpiration,
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(2)
            };
            _memoryCache.Set(key, value, options);
        }
        return Task.CompletedTask;
    }
}