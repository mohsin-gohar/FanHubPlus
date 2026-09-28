using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class ChannelService : IChannelService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ChannelService> _logger;

    private const string CacheKeyPrefix = "channel_";
    private static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public ChannelService(FanHubDbContext db, IMemoryCache cache, ILogger<ChannelService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Channel?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}byid_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Channels.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        })!;
    }

    public async Task<PagedResult<Channel>> GetPagedAsync(ChannelFilter filter)
    {
        var cacheKey = $"{CacheKeyPrefix}paged_{filter.GetCacheKey()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var query = _db.Channels.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All")
            {
                query = query.Where(c => c.Category == filter.Category);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.Viewers)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var genres = await _db.Channels
                .AsNoTracking()
                .Select(c => c.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return new PagedResult<Channel>
            {
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize,
                AdditionalData = new ChannelListData
                {
                    Genres = genres
                }
            };
        })!;
    }

    public async Task<List<Channel>> GetTopAsync(int count = 6)
    {
        var cacheKey = $"{CacheKeyPrefix}top_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Channels
                .AsNoTracking()
                .OrderByDescending(c => c.Viewers)
                .Take(count)
                .ToListAsync();
        })!;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        const string cacheKey = $"{CacheKeyPrefix}categories";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Channels
                .AsNoTracking()
                .Select(c => c.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        })!;
    }
}