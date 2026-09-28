using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class FaqService : IFaqService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FaqService> _logger;

    private const string CacheKeyPrefix = "faq_";
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public FaqService(FanHubDbContext db, IMemoryCache cache, ILogger<FaqService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<FaqItem>> GetAllAsync(string? topic = null)
    {
        var cacheKey = $"{CacheKeyPrefix}all_{topic ?? "all"}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            var query = _db.Faqs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(topic) && topic != "All")
            {
                query = query.Where(f => f.Topic == topic);
            }

            return await query.OrderBy(f => f.Id).ToListAsync();
        })!;
    }

    public async Task<List<string>> GetTopicsAsync()
    {
        const string cacheKey = $"{CacheKeyPrefix}topics";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Faqs
                .AsNoTracking()
                .Select(f => f.Topic)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        })!;
    }

    public async Task<List<FaqItem>> GetPopularAsync(int count = 4)
    {
        var cacheKey = $"{CacheKeyPrefix}popular_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Faqs
                .AsNoTracking()
                .Where(f => f.IsPopular)
                .Take(count)
                .ToListAsync();
        })!;
    }
}