using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class BlogService : IBlogService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BlogService> _logger;

    private const string CacheKeyPrefix = "blog_";
    private static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public BlogService(FanHubDbContext db, IMemoryCache cache, ILogger<BlogService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BlogPost?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}byid_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        })!;
    }

    public async Task<BlogDetailDto?> GetDetailAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}detail_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var post = await _db.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
            if (post == null) return null;

            var allPosts = await _db.BlogPosts.AsNoTracking().ToListAsync();

            var related = allPosts
                .Where(b => b.Id != id)
                .OrderBy(b => b.Category == post.Category ? 0 : 1)
                .ThenByDescending(b => b.PublishedOn)
                .Take(3)
                .ToList();

            var latest = allPosts
                .OrderByDescending(b => b.PublishedOn)
                .Take(4)
                .ToList();

            var popular = allPosts
                .OrderByDescending(b => b.Comments)
                .Take(4)
                .ToList();

            var comments = await _db.Comments
                .AsNoTracking()
                .Where(c => c.VideoId == 0)
                .OrderByDescending(c => c.PostedOn)
                .ToListAsync();

            return new BlogDetailDto
            {
                Post = post,
                Related = related,
                Latest = latest,
                Popular = popular,
                Comments = comments
            };
        })!;
    }

    public async Task<PagedResult<BlogPost>> GetPagedAsync(BlogFilter filter)
    {
        var cacheKey = $"{CacheKeyPrefix}paged_{filter.GetCacheKey()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var query = _db.BlogPosts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "All")
            {
                query = query.Where(b => b.Category == filter.Category);
            }

            var total = await query.CountAsync();

            var pageSize = filter.PageSize > 0 ? filter.PageSize : 6;
            var currentPage = Math.Max(1, filter.Page);

            var allPosts = await _db.BlogPosts.AsNoTracking().ToListAsync();

            var items = await query
                .OrderByDescending(b => b.PublishedOn)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var latest = allPosts.OrderByDescending(b => b.PublishedOn).Take(4).ToList();
            var popular = allPosts.OrderByDescending(b => b.Comments).Take(4).ToList();
            var categories = allPosts.Select(b => b.Category).Distinct().OrderBy(c => c).ToList();
            var tags = allPosts.SelectMany(b => b.TagList)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(14)
                .ToList();

            return new PagedResult<BlogPost>
            {
                Items = items,
                TotalCount = total,
                Page = currentPage,
                PageSize = pageSize,
                AdditionalData = new BlogListData
                {
                    Latest = latest,
                    Popular = popular,
                    Categories = categories,
                    Tags = tags
                }
            };
        })!;
    }

    public async Task<List<BlogPost>> GetLatestAsync(int count = 4)
    {
        var cacheKey = $"{CacheKeyPrefix}latest_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.BlogPosts
                .AsNoTracking()
                .OrderByDescending(b => b.PublishedOn)
                .Take(count)
                .ToListAsync();
        })!;
    }

    public async Task<List<BlogPost>> GetPopularAsync(int count = 4)
    {
        var cacheKey = $"{CacheKeyPrefix}popular_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.BlogPosts
                .AsNoTracking()
                .OrderByDescending(b => b.Comments)
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
            return await _db.BlogPosts
                .AsNoTracking()
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        })!;
    }
}