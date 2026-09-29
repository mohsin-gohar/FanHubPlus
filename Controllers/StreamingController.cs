using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

/// <summary>
/// Dedicated Movies &amp; TV Shows streaming library.
/// Serves Movie, Series, and Documentary content types from the shared Content table.
/// </summary>
public class StreamingController : Controller
{
    // ViewLog.ItemType used by the "Seen" button - a lightweight watch history
    // so the marker works for signed-in fans without a new table.
    private const string SeenItemType = "ContentSeen";

    private static readonly ContentType[] Playing =
        [ContentType.Movie, ContentType.Series, ContentType.Documentary, ContentType.Special];

    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Rating> _ratings;
    private readonly IRepository<ViewLog> _views;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IContentService _contentService;
    private readonly IStatsService _stats;

    public StreamingController(IRepository<Content> contents,
                               IRepository<Category> categories,
                               IRepository<Rating> ratings,
                               IRepository<ViewLog> views,
                               UserManager<ApplicationUser> userManager,
                               IBookmarkService bookmarks,
                               IContentService contentService,
                               IStatsService stats)
    {
        _contents       = contents;
        _categories     = categories;
        _ratings        = ratings;
        _views          = views;
        _userManager    = userManager;
        _bookmarks      = bookmarks;
        _contentService = contentService;
        _stats          = stats;
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
    // The page mirrors the template's "movie-tv-show-details" layout, so the view
    // model carries everything its sections need: meta sidebar, cast, reviews,
    // vote counters and the "featured" band under the page.
    public async Task<IActionResult> Details(int id)
    {
        var item = await _contents.QueryTracked()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (item is null) return NotFound();

        // Limit to streaming content only
        if (!Playing.Contains(item.Type)) return NotFound();

        var userId = (await _userManager.GetUserAsync(User))?.Id;

        item.ViewCount++;
        await _contents.SaveChangesAsync();
        await _stats.LogViewAsync("Streaming", item.ContentId, userId);

        return View(await BuildDetailAsync(item, userId));
    }

    /// <summary>Loads every collection / counter the detail view renders.</summary>
    private async Task<StreamingDetailViewModel> BuildDetailAsync(Content item, string? userId)
    {
        var id = item.ContentId;

        // SelectMany keeps this one SQL round-trip (no N+1 over Content.Ratings)
        var ratingRows = _contents.Query().AsNoTracking()
            .Where(c => c.ContentId == id)
            .SelectMany(c => c.Ratings);

        var ratingCount = await ratingRows.CountAsync();
        var averageRating = ratingCount == 0
            ? 0
            : Math.Round(await ratingRows.AverageAsync(r => (double)r.Stars), 1);

        var reviews = await ratingRows
            .Where(r => r.Review != null && r.Review != "")
            .OrderByDescending(r => r.CreatedAt)
            .Take(12)
            .Select(r => new ContentReviewViewModel
            {
                Author    = r.User.Name ?? r.User.UserName ?? "FanHubPlus member",
                CreatedAt = r.CreatedAt,
                Stars     = r.Stars,
                Text      = r.Review
            })
            .ToListAsync();

        // The cards carry a portrait; the seeded set is decorative, so give each
        // card a stable one instead of shipping a "no avatar" hole.
        for (var i = 0; i < reviews.Count; i++)
            reviews[i].AvatarUrl = $"/assets/images/users/user{(i % 8) + 1}.jpg";

        var vm = new StreamingDetailViewModel
        {
            Item          = item,
            // The main player prefers a full video and falls back to a trailer.
            Trailer       = item.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Video)
                            ?? item.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Trailer),
            AverageRating = averageRating,
            RatingCount   = ratingCount,
            Reviews       = reviews,
            UpVotes       = await ratingRows.CountAsync(r => r.Stars >= 4),
            DownVotes     = await ratingRows.CountAsync(r => r.Stars <= 2),
            SeenCount     = await _views.Query().AsNoTracking()
                                .CountAsync(v => v.ItemType == SeenItemType && v.ItemId == id),
            Related       = await _contents.Query().AsNoTracking()
                .Include(c => c.Category)
                .Where(c => Playing.Contains(c.Type) && c.ContentId != id)
                .OrderByDescending(c => c.PopularityScore)
                .Take(8)
                .ToListAsync(),
            Featured      = await _contents.Query().AsNoTracking()
                .Include(c => c.Category)
                .Where(c => Playing.Contains(c.Type))
                .OrderByDescending(c => c.PopularityScore)
                .Take(4)
                .ToListAsync()
        };

        // One grouped query gives the "Related Movies" cards their star counts.
        var relatedIds = vm.Related.Select(r => r.ContentId).ToList();
        vm.RelatedRatings = (await _contents.Query().AsNoTracking()
                .Where(c => relatedIds.Contains(c.ContentId))
                .Select(c => new
                {
                    c.ContentId,
                    Count = c.Ratings.Count,
                    Avg = c.Ratings.Count == 0 ? 0 : c.Ratings.Average(r => (double)r.Stars)
                })
                .ToListAsync())
            .ToDictionary(x => x.ContentId, x => (x.Avg, x.Count));

        if (userId is not null)
        {
            vm.IsBookmarked = await _bookmarks.IsBookmarkedAsync(userId, BookmarkType.Content, id);
            vm.MyStars      = await ratingRows
                .Where(r => r.UserId == userId)
                .Select(r => (int?)r.Stars)
                .FirstOrDefaultAsync();
            vm.IsSeen = await _views.Query().AsNoTracking()
                .AnyAsync(v => v.ItemType == SeenItemType && v.ItemId == id && v.UserId == userId);
        }

        return vm;
    }

    // POST /Streaming/Review  (the "write a review" form in the Reviews block)
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("write")]
    public async Task<IActionResult> Review(int contentId, int stars, string? review)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return RedirectToAction("Login", "Account",
                new { returnUrl = Url.Action(nameof(Details), new { id = contentId }) });

        stars = Math.Clamp(stars, 1, 5);
        var text = string.IsNullOrWhiteSpace(review) ? null : review.Trim();
        if (text is { Length: > 1000 }) text = text[..1000];

        var existing = await _ratings.QueryTracked()
            .FirstOrDefaultAsync(r => r.UserId == user.Id && r.ContentId == contentId);

        if (existing is null)
        {
            await _ratings.AddAsync(new Rating
            {
                UserId    = user.Id,
                ContentId = contentId,
                Stars     = stars,
                Review    = text,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Stars = stars;
            // An empty box only updates the stars - it never wipes a written review.
            if (text is not null) existing.Review = text;
        }

        await _ratings.SaveChangesAsync();

        TempData["StatusMessage"] = text is null
            ? "Your star rating was saved."
            : "Thanks! Your review is now live.";
        return Redirect(Url.Action(nameof(Details), new { id = contentId }) + "#reviews");
    }

    // POST /Streaming/Vote  (AJAX: the sidebar thumbs-up / thumbs-down buttons,
    // the star widget on the page and the "rate this" control in the meta panel)
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("write")]
    public async Task<IActionResult> Vote(int contentId, int stars)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Json(new { ok = false, auth = false, message = "Please log in to rate this title." });

        try
        {
            var (avg, count, myStars) = await _contentService.RateAsync(user.Id, contentId, stars);

            var rows = _contents.Query().AsNoTracking()
                .Where(c => c.ContentId == contentId)
                .SelectMany(c => c.Ratings);

            return Json(new
            {
                ok = true,
                avg,
                count,
                myStars,
                up   = await rows.CountAsync(r => r.Stars >= 4),
                down = await rows.CountAsync(r => r.Stars <= 2)
            });
        }
        catch (ArgumentOutOfRangeException)
        {
            return Json(new { ok = false, message = "Stars must be between 1 and 5." });
        }
        catch (KeyNotFoundException)
        {
            return Json(new { ok = false, message = "That title is no longer available." });
        }
    }

    // POST /Streaming/ToggleSeen  (AJAX from the sidebar "Seen" button)
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("write")]
    public async Task<IActionResult> ToggleSeen(int contentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Json(new { ok = false, auth = false, message = "Please log in to keep your watch history." });

        var existing = await _views.QueryTracked()
            .FirstOrDefaultAsync(v => v.ItemType == SeenItemType && v.ItemId == contentId && v.UserId == user.Id);

        bool seen;
        if (existing is null)
        {
            await _views.AddAsync(new ViewLog
            {
                ItemType = SeenItemType,
                ItemId   = contentId,
                UserId   = user.Id,
                ViewedAt = DateTime.UtcNow
            });
            seen = true;
        }
        else
        {
            _views.Remove(existing);
            seen = false;
        }
        await _views.SaveChangesAsync();

        var count = await _views.Query().AsNoTracking()
            .CountAsync(v => v.ItemType == SeenItemType && v.ItemId == contentId);

        return Json(new { ok = true, seen, count });
    }
}
