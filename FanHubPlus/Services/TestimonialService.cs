using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class TestimonialService : ITestimonialService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TestimonialService> _logger;

    private const string CacheKeyPrefix = "testimonial_";
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public TestimonialService(FanHubDbContext db, IMemoryCache cache, ILogger<TestimonialService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Testimonial>> GetAllAsync()
    {
        const string cacheKey = $"{CacheKeyPrefix}all";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Testimonials.AsNoTracking().ToListAsync();
        })!;
    }
}