using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class StoreService : IStoreService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StoreService> _logger;

    private const string CacheKeyPrefix = "store_";
    private static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public StoreService(FanHubDbContext db, IMemoryCache cache, ILogger<StoreService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<StoreProduct?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}product_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        })!;
    }

    public async Task<StoreDetailDto?> GetDetailAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}detail_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;

            var allProducts = await _db.Products.AsNoTracking().ToListAsync();

            var related = allProducts
                .Where(p => p.Id != id)
                .OrderByDescending(p => p.Sold)
                .Take(4)
                .ToList();

            var sameCategory = allProducts
                .Where(p => p.Id != id && p.Category == product.Category)
                .Take(3)
                .ToList();

            var gallery = new List<StoreProduct> { product }
                .Concat(allProducts.Where(p => p.ArtTheme == product.ArtTheme && p.Id != id).Take(2))
                .ToList();

            return new StoreDetailDto
            {
                Product = product,
                Related = related,
                SameCategory = sameCategory,
                Gallery = gallery
            };
        })!;
    }

    public async Task<PagedResult<StoreProduct>> GetPagedAsync(StoreFilter filter)
    {
        var cacheKey = $"{CacheKeyPrefix}paged_{filter.GetCacheKey()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var query = _db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All")
            {
                query = query.Where(p => p.Category == filter.Category);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
            }

            if (filter.MinPrice.HasValue) query = query.Where(p => p.Price >= filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            var allProducts = await _db.Products.AsNoTracking().ToListAsync();
            var filtered = query.ToList();

            filtered = filter.SortBy switch
            {
                "price-asc" => filtered.OrderBy(p => p.Price).ToList(),
                "price-desc" => filtered.OrderByDescending(p => p.Price).ToList(),
                "rating" => filtered.OrderByDescending(p => p.Rating).ToList(),
                _ => filtered.OrderByDescending(p => p.Sold).ToList()
            };

            var featured = allProducts
                .OrderByDescending(p => p.DiscountPercent)
                .Take(3)
                .ToList();

            var categories = allProducts
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return new PagedResult<StoreProduct>
            {
                Items = filtered,
                TotalCount = filtered.Count,
                Page = 1,
                PageSize = filtered.Count,
                AdditionalData = new StoreListData
                {
                    Featured = featured,
                    Categories = categories
                }
            };
        })!;
    }

    public async Task<List<StoreProduct>> GetFeaturedAsync(int count = 3)
    {
        var cacheKey = $"{CacheKeyPrefix}featured_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.DiscountPercent)
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
            return await _db.Products
                .AsNoTracking()
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        })!;
    }
}