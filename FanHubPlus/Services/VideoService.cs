using System.Linq;
using FanHubPlus.Data;
using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FanHubPlus.Services;

public class VideoService : IVideoService
{
    private readonly FanHubDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<VideoService> _logger;

    private const string CacheKeyPrefix = "video_";
    private static readonly TimeSpan ShortCache = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MediumCache = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LongCache = TimeSpan.FromMinutes(30);

    public VideoService(FanHubDbContext db, IMemoryCache cache, ILogger<VideoService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<VideoItem?> GetByIdAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}byid_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        })!;
    }

    public async Task<VideoDetailDto?> GetDetailAsync(int id)
    {
        var cacheKey = $"{CacheKeyPrefix}detail_{id}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;

            var video = await _db.Videos.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return null;

            var episodes = await _db.Episodes
                .AsNoTracking()
                .Where(e => e.VideoId == id)
                .OrderBy(e => e.Season).ThenBy(e => e.Number)
                .ToListAsync();

            var related = await _db.Videos
                .AsNoTracking()
                .Where(v => v.Id != id && (v.Genre == video.Genre || v.IsTopRated))
                .Take(6)
                .ToListAsync();

            var comments = await _db.Comments
                .AsNoTracking()
                .Where(c => c.VideoId == id && c.ParentId == null)
                .OrderByDescending(c => c.PostedOn)
                .ToListAsync();

            var replies = await _db.Comments
                .AsNoTracking()
                .Where(c => c.VideoId == id && c.ParentId != null)
                .ToListAsync();

            return new VideoDetailDto
            {
                Video = video,
                Episodes = episodes,
                Related = related,
                Comments = comments,
                Replies = replies
            };
        })!;
    }

    public async Task<PagedResult<VideoListDto>> GetPagedAsync(VideoFilter filter)
    {
        var cacheKey = $"{CacheKeyPrefix}paged_{filter.GetCacheKey()}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = ShortCache;

            var query = _db.Videos.AsNoTracking().AsQueryable();

            if (filter.Type == VideoType.TvShows) query = query.Where(v => v.IsSeries);
            else if (filter.Type == VideoType.Live) query = query.Where(v => v.IsLive);

            if (!string.IsNullOrWhiteSpace(filter.Genre) && filter.Genre != "All")
            {
                query = query.Where(v => v.Genre.ToLower() == filter.Genre.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.ToLower();
                query = query.Where(v => v.Title.ToLower().Contains(term)
                    || v.Description.ToLower().Contains(term)
                    || v.Genre.ToLower().Contains(term)
                    || v.Cast.ToLower().Contains(term));
            }

            var total = await query.CountAsync();

            query = filter.SortBy switch
            {
                "rating" => query.OrderByDescending(v => v.ImdbRating),
                "year" => query.OrderByDescending(v => v.Year),
                "title" => query.OrderBy(v => v.Title),
                "oldest" => query.OrderBy(v => v.ReleaseDate),
                _ => query.OrderByDescending(v => v.ReleaseDate)
            };

            var pageSize = filter.PageSize > 0 ? filter.PageSize : 12;
            var currentPage = Math.Max(1, filter.Page);

            var items = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsFeatured = v.IsFeatured,
                    IsTrending = v.IsTrending,
                    IsTopRated = v.IsTopRated,
                    IsLive = v.IsLive,
                    WatchingNow = v.WatchingNow,
                    PosterTheme = v.PosterTheme,
                    ReleaseDate = v.ReleaseDate
                })
                .ToListAsync();

            return new PagedResult<VideoListDto>
            {
                Items = items,
                TotalCount = total,
                Page = currentPage,
                PageSize = pageSize
            };
        })!;
    }

    public async Task<List<VideoListDto>> GetFeaturedAsync(int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}featured_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.IsFeatured)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsFeatured = v.IsFeatured,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetTrendingAsync(int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}trending_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.IsTrending)
                .OrderByDescending(v => v.ImdbRating)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsTrending = v.IsTrending,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetTopRatedAsync(int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}toprated_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.IsTopRated)
                .OrderByDescending(v => v.ImdbRating)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsTopRated = v.IsTopRated,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetLatestAsync(int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}latest_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Videos
                .AsNoTracking()
                .OrderByDescending(v => v.ReleaseDate)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    PosterTheme = v.PosterTheme,
                    ReleaseDate = v.ReleaseDate
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetLiveAsync(int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}live_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = ShortCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.IsLive)
                .OrderByDescending(v => v.WatchingNow)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsLive = v.IsLive,
                    WatchingNow = v.WatchingNow,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetByGenreAsync(string genre, int count = 10)
    {
        var cacheKey = $"{CacheKeyPrefix}genre_{genre}_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.Genre.ToLower() == genre.ToLower())
                .OrderByDescending(v => v.ReleaseDate)
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<VideoListDto>> GetRelatedAsync(int videoId, int count = 6)
    {
        var video = await GetByIdAsync(videoId);
        if (video == null) return new List<VideoListDto>();

        var cacheKey = $"{CacheKeyPrefix}related_{videoId}_{video.Genre}_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Videos
                .AsNoTracking()
                .Where(v => v.Id != videoId && (v.Genre == video.Genre || v.IsTopRated))
                .Take(count)
                .Select(v => new VideoListDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    Genre = v.Genre,
                    Year = v.Year,
                    Duration = v.Duration,
                    ImdbRating = v.ImdbRating,
                    IsTopRated = v.IsTopRated,
                    PosterTheme = v.PosterTheme
                })
                .ToListAsync();
        })!;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        const string cacheKey = $"{CacheKeyPrefix}categories";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = LongCache;
            return await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        })!;
    }

    public async Task<List<Channel>> GetChannelsAsync(string? category = null)
    {
        var cacheKey = $"{CacheKeyPrefix}channels_{category ?? "all"}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            var query = _db.Channels.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(c => c.Category == category);
            }
            return await query.OrderByDescending(c => c.Viewers).ToListAsync();
        })!;
    }

    public async Task<List<Channel>> GetTopChannelsAsync(int count = 6)
    {
        var cacheKey = $"{CacheKeyPrefix}topchannels_{count}";
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MediumCache;
            return await _db.Channels
                .AsNoTracking()
                .OrderByDescending(c => c.Viewers)
                .Take(count)
                .ToListAsync();
        })!;
    }

    public async Task IncrementViewCountAsync(int videoId)
    {
        await _cache.RemoveAsync($"{CacheKeyPrefix}byid_{videoId}");
        await _cache.RemoveAsync($"{CacheKeyPrefix}detail_{videoId}");
    }
}