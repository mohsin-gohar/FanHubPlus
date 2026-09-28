using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

/// <summary>
/// Dedicated Movies &amp; TV Shows streaming library.
/// Serves Movie, Series, and Documentary content types from the shared Content table.
/// </summary>
public class StreamingController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public StreamingController(IRepository<Content> contents,
                               IRepository<Category> categories,
                               UserManager<ApplicationUser> userManager,
                               IBookmarkService bookmarks,
                               IStatsService stats)
    {
        _contents   = contents;
        _categories = categories;
        _userManager = userManager;
        _bookmarks  = bookmarks;
        _stats      = stats;
    }

    // GET /Streaming
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> Index(
        string? search, int? categoryId,
        string? genre, string? contentType,
        string? sort, int page = 1)
    {
        var vm = new StreamingViewModel
        {
            Search      = search,
            CategoryId  = categoryId,
            Genre       = genre,
            ContentType = contentType,
            Sort        = sort ?? "popular",
            Page        = Math.Max(1, page),
            Categories  = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        // Only streaming-relevant types
        var streamingTypes = new[] { ContentType.Movie, ContentType.Series, ContentType.Documentary, ContentType.Special };

        var query = _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => streamingTypes.Contains(c.Type))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(s) ||
                (c.Genre != null && c.Genre.ToLower().Contains(s)) ||
                (c.Description != null && c.Description.ToLower().Contains(s)));
        }

        if (categoryId is > 0)
            query = query.Where(c => c.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(c => c.Genre != null && c.Genre.ToLower().Contains(genre.ToLower()));

        if (!string.IsNullOrWhiteSpace(contentType) && Enum.TryParse<ContentType>(contentType, out var ct))
            query = query.Where(c => c.Type == ct);

        query = vm.Sort switch
        {
            "newest"  => query.OrderByDescending(c => c.ReleaseDate),
            "views"   => query.OrderByDescending(c => c.ViewCount),
            "title"   => query.OrderBy(c => c.Title),
            _         => query.OrderByDescending(c => c.PopularityScore)
        };

        vm.TotalItems = await query.CountAsync();
        vm.Items = await query
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        // Featured hero: top 5 by popularity for carousel
        vm.Featured = await _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => streamingTypes.Contains(c.Type))
            .OrderByDescending(c => c.PopularityScore)
            .Take(5)
            .ToListAsync();

        return View(vm);
    }

    // GET /Streaming/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var item = await _contents.QueryTracked()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (item is null) return NotFound();

        // Limit to streaming content only
        var streamingTypes = new[] { ContentType.Movie, ContentType.Series, ContentType.Documentary, ContentType.Special };
        if (!streamingTypes.Contains(item.Type)) return NotFound();

        var userId = (await _userManager.GetUserAsync(User))?.Id;
        item.ViewCount++;
        await _contents.SaveChangesAsync();
        await _stats.LogViewAsync("Streaming", item.ContentId, userId);

        var starsQuery = _contents.Query().AsNoTracking()
            .Where(c => c.ContentId == id)
            .SelectMany(c => c.Ratings.Select(r => r.Stars));

        var ratingCount    = await starsQuery.CountAsync();
        var averageRating  = ratingCount == 0 ? 0 : Math.Round(await starsQuery.AverageAsync(s => (double)s), 1);

        var vm = new StreamingDetailViewModel
        {
            Item          = item,
            Trailer       = item.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Trailer)
                            ?? item.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Video),
            AverageRating = averageRating,
            RatingCount   = ratingCount,
            Related       = await _contents.Query().AsNoTracking()
                .Include(c => c.Category)
                .Where(c => streamingTypes.Contains(c.Type) && c.ContentId != id)
                .OrderByDescending(c => c.PopularityScore)
                .Take(6)
                .ToListAsync()
        };

        if (userId is not null)
            vm.IsBookmarked = await _bookmarks.IsBookmarkedAsync(userId, BookmarkType.Content, id);

        return View(vm);
    }
}
