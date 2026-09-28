using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class CareerService : ICareerService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CareerService> _logger;

    private const string CacheKeyPrefix = "career_";
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public CareerService(FanHubDbContext db, IMemoryCache cache, ILogger<CareerService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<JobOpening?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}byid_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
        })!;
    }

    public async Task<List<JobOpening>> GetAllAsync()
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Jobs.AsNoTracking().OrderByDescending(j => j.PostedOn).ToListAsync();
        })!;
    }

    public async Task<List<JobOpening>> GetOtherJobsAsync(int excludeId, int count = 4)
    {
        var cacheKey = $"{CacheKeyPrefix}others_{excludeId}_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Jobs
                .AsNoTracking()
                .Where(j => j.Id != excludeId)
                .OrderByDescending(j => j.PostedOn)
                .Take(count)
                .ToListAsync();
        })!;
    }
}