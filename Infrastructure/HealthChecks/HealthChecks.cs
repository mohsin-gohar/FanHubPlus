using System;
using System.Threading;
using System.Threading.Tasks;
using FanHubPlus.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace FanHubPlus.Infrastructure.HealthChecks;

/// <summary>
/// Health check for the multi-tier cache (Memory + Redis).
/// </summary>
public class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<CacheHealthCheck> _logger;

    public CacheHealthCheck(
        ICacheService cache,
        IDistributedCache? distributedCache,
        ILogger<CacheHealthCheck> logger)
    {
        _cache = cache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        // Test L1 (Memory) cache
        var l1Key = $"health:l1:{Guid.NewGuid()}";
        var l1TestValue = "test";
        await _cache.SetAsync(l1Key, l1TestValue, TimeSpan.FromSeconds(10));
        var l1Result = await _cache.GetAsync<string>(l1Key);
        await _cache.RemoveAsync(l1Key);

        data["L1_MemoryCache"] = l1Result == l1TestValue ? "Healthy" : "Degraded";

        if (l1Result != l1TestValue)
        {
            return HealthCheckResult.Degraded("L1 MemoryCache read/write failed", data: data);
        }

        // Test L2 (Redis) cache if available
        if (_distributedCache is not null)
        {
            try
            {
                var l2Key = $"health:l2:{Guid.NewGuid()}";
                var l2TestValue = "test";
                await _distributedCache.SetStringAsync(l2Key, l2TestValue, cancellationToken);
                var l2Result = await _distributedCache.GetStringAsync(l2Key, cancellationToken);
                await _distributedCache.RemoveAsync(l2Key, cancellationToken);

                data["L2_RedisCache"] = l2Result == l2TestValue ? "Healthy" : "Degraded";

                if (l2Result != l2TestValue)
                {
                    return HealthCheckResult.Degraded("L2 RedisCache read/write failed", data: data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis health check failed");
                data["L2_RedisCache"] = "Unhealthy";
                data["L2_Error"] = ex.Message;
                return HealthCheckResult.Degraded("L2 RedisCache unavailable", ex, data);
            }
        }
        else
        {
            data["L2_RedisCache"] = "NotConfigured";
        }

        return HealthCheckResult.Healthy("Cache tiers operational", data);
    }
}

/// <summary>
/// Health check for database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly FanHubPlus.Data.ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        FanHubPlus.Data.ApplicationDbContext dbContext,
        ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Test a simple query
            var categoryCount = await _dbContext.Categories.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["CategoryCount"] = categoryCount,
                ["Provider"] = _dbContext.Database.ProviderName ?? "Unknown"
            };

            return HealthCheckResult.Healthy("Database connection successful", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

/// <summary>
/// Health check for external API dependencies (YouTube, etc.)
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalApiHealthCheck> _logger;

    public ExternalApiHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalApiHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var client = _httpClientFactory.CreateClient("ExternalApiCheck");
        client.Timeout = TimeSpan.FromSeconds(5);

        // Check YouTube embed endpoint
        try
        {
            var response = await client.GetAsync("https://www.youtube.com/embed/MGRm4IzK1SQ", cancellationToken);
            data["YouTube"] = response.IsSuccessStatusCode ? "Healthy" : $"Degraded ({(int)response.StatusCode})";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "YouTube health check failed");
            data["YouTube"] = "Unhealthy";
        }

        // Check Vimeo
        try
        {
            var response = await client.GetAsync("https://player.vimeo.com/video/123456", cancellationToken);
            data["Vimeo"] = response.IsSuccessStatusCode ? "Healthy" : $"Degraded ({(int)response.StatusCode})";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vimeo health check failed");
            data["Vimeo"] = "Unhealthy";
        }

        var allHealthy = data.Values.All(v => v.ToString()?.StartsWith("Healthy") == true);
        return allHealthy
            ? HealthCheckResult.Healthy("External APIs reachable", data)
            : HealthCheckResult.Degraded("Some external APIs unreachable", data: data);
    }
}

/// <summary>
/// Startup health check that runs once on application start.
/// </summary>
public class StartupHealthCheck : IHealthCheck
{
    private readonly ILogger<StartupHealthCheck> _logger;
    private bool _startupComplete = false;

    public StartupHealthCheck(ILogger<StartupHealthCheck> logger)
    {
        _logger = logger;
    }

    public void MarkStartupComplete() => _startupComplete = true;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_startupComplete)
        {
            return Task.FromResult(HealthCheckResult.Degraded("Application still starting up"));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Application started successfully"));
    }
}