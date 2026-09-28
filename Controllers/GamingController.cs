using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

public class GamingController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public GamingController(IRepository<Content> contents,
                            IRepository<Category> categories,
                            UserManager<ApplicationUser> userManager,
                            IBookmarkService bookmarks,
                            IStatsService stats)
    {
        _contents = contents;
        _categories = categories;
        _userManager = userManager;
        _bookmarks = bookmarks;
        _stats = stats;
    }

    // GET /Gaming
    public async Task<IActionResult> Index(string? search, int? categoryId, bool playableOnly = false, string? sort = "popular", int page = 1)
    {
        var vm = new GamingViewModel
        {
            Search = search,
            CategoryId = categoryId,
            PlayableOnly = playableOnly,
            Sort = sort ?? "popular",
            Page = Math.Max(1, page),
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => c.Type == ContentType.Game || c.Type == ContentType.PlayableGame || c.Category.Name == "Gaming")
            .AsQueryable();

        if (playableOnly)
        {
            query = query.Where(c => c.Type == ContentType.PlayableGame || c.PlayableGameUrl != null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(s) ||
                (c.Genre != null && c.Genre.ToLower().Contains(s)) ||
                (c.Description != null && c.Description.ToLower().Contains(s)));
        }

        if (categoryId is > 0) query = query.Where(c => c.CategoryId == categoryId);

        query = vm.Sort switch
        {
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            "title" => query.OrderBy(c => c.Title),
            _ => query.OrderByDescending(c => c.PopularityScore)
        };

        vm.TotalItems = await query.CountAsync();
        vm.Games = await query
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        return View(vm);
    }

    // GET /Gaming/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var game = await _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (game is null) return NotFound();

        var userId = (await _userManager.GetUserAsync(User))?.Id;
        game.ViewCount++;
        await _contents.SaveChangesAsync();
        await _stats.LogViewAsync("Gaming", game.ContentId, userId);

        var starsQuery = _contents.Query().AsNoTracking()
            .Where(c => c.ContentId == id)
            .SelectMany(c => c.Ratings.Select(r => r.Stars));

        var ratingCount = await starsQuery.CountAsync();
        var averageRating = ratingCount == 0 ? 0 : Math.Round(await starsQuery.AverageAsync(s => (double)s), 1);

        var vm = new GameDetailViewModel
        {
            Game = game,
            Trailer = game.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Trailer)
                      ?? game.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Video),
            AverageRating = averageRating,
            RatingCount = ratingCount,
            RelatedGames = await _contents.Query().AsNoTracking()
                .Where(c => (c.Type == ContentType.Game || c.Type == ContentType.PlayableGame || c.Category.Name == "Gaming") && c.ContentId != id)
                .OrderByDescending(c => c.PopularityScore)
                .Take(4)
                .ToListAsync()
        };

        if (userId is not null)
        {
            vm.IsBookmarked = await _bookmarks.IsBookmarkedAsync(userId, BookmarkType.Content, id);
        }

        return View(vm);
    }

    // GET /Gaming/Play/5
    public async Task<IActionResult> Play(int id)
    {
        var game = await _contents.Query()
            .FirstOrDefaultAsync(c => c.ContentId == id);

        if (game is null || string.IsNullOrWhiteSpace(game.PlayableGameUrl))
        {
            TempData["StatusError"] = "This game does not support direct browser play.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = (await _userManager.GetUserAsync(User))?.Id;
        await _stats.LogViewAsync("PlayableGame", game.ContentId, userId);

        return View(game);
    }
}
