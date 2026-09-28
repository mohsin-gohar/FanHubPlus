using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

public class MusicController : Controller
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Category> _categories;
    private readonly IRepository<Playlist> _playlists;
    private readonly IRepository<PlaylistItem> _playlistItems;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookmarkService _bookmarks;
    private readonly IStatsService _stats;

    public MusicController(IRepository<Content> contents,
                           IRepository<Category> categories,
                           IRepository<Playlist> playlists,
                           IRepository<PlaylistItem> playlistItems,
                           UserManager<ApplicationUser> userManager,
                           IBookmarkService bookmarks,
                           IStatsService stats)
    {
        _contents = contents;
        _categories = categories;
        _playlists = playlists;
        _playlistItems = playlistItems;
        _userManager = userManager;
        _bookmarks = bookmarks;
        _stats = stats;
    }

    // GET /Music
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> Index(string? search, int? categoryId, string? sort, int page = 1)
    {
        var vm = new MusicViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Sort = sort ?? "popular",
            Page = Math.Max(1, page),
            Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync()
        };

        var query = _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => c.Type == ContentType.Song || c.Type == ContentType.Album)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(s) ||
                (c.Artist != null && c.Artist.ToLower().Contains(s)) ||
                (c.AlbumName != null && c.AlbumName.ToLower().Contains(s)) ||
                (c.Genre != null && c.Genre.ToLower().Contains(s)));
        }

        if (categoryId is > 0) query = query.Where(c => c.CategoryId == categoryId);

        query = vm.Sort switch
        {
            "newest" => query.OrderByDescending(c => c.CreatedAt),
            "artist" => query.OrderBy(c => c.Artist ?? c.Title),
            "title" => query.OrderBy(c => c.Title),
            _ => query.OrderByDescending(c => c.PopularityScore)
        };

        vm.TotalItems = await query.CountAsync();
        vm.Songs = await query
            .Skip((vm.Page - 1) * vm.PageSize)
            .Take(vm.PageSize)
            .ToListAsync();

        vm.PublicPlaylists = await _playlists.Query()
            .Include(p => p.User)
            .Include(p => p.Items)
            .Where(p => p.IsPublic)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();

        return View(vm);
    }

    // GET /Music/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var song = await _contents.QueryTracked()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .FirstOrDefaultAsync(c => c.ContentId == id && (c.Type == ContentType.Song || c.Type == ContentType.Album));

        if (song is null) return NotFound();

        var userId = (await _userManager.GetUserAsync(User))?.Id;
        song.ViewCount++;
        await _contents.SaveChangesAsync();
        await _stats.LogViewAsync("Music", song.ContentId, userId);

        var starsQuery = _contents.Query().AsNoTracking()
            .Where(c => c.ContentId == id)
            .SelectMany(c => c.Ratings.Select(r => r.Stars));

        var ratingCount = await starsQuery.CountAsync();
        var averageRating = ratingCount == 0 ? 0 : Math.Round(await starsQuery.AverageAsync(s => (double)s), 1);

        var vm = new MusicDetailViewModel
        {
            Song = song,
            MainAudio = song.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Audio)
                        ?? song.MediaItems.FirstOrDefault(m => m.MediaType == MediaType.Video),
            AverageRating = averageRating,
            RatingCount = ratingCount,
            RelatedSongs = await _contents.Query().AsNoTracking()
                .Where(c => (c.Type == ContentType.Song || c.Type == ContentType.Album) && c.ContentId != id)
                .OrderByDescending(c => c.PopularityScore)
                .Take(4)
                .ToListAsync()
        };

        if (userId is not null)
        {
            vm.IsBookmarked = await _bookmarks.IsBookmarkedAsync(userId, BookmarkType.Content, id);
            vm.MyPlaylists = await _playlists.Query()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        return View(vm);
    }

    // GET /Music/Playlists
    [Authorize]
    public async Task<IActionResult> Playlists()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var userPlaylists = await _playlists.Query()
            .Include(p => p.Items).ThenInclude(pi => pi.Content)
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(userPlaylists);
    }

    // POST /Music/CreatePlaylist
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePlaylist(string title, string? description)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["StatusError"] = "Playlist name cannot be empty.";
            return RedirectToAction(nameof(Playlists));
        }

        var playlist = new Playlist
        {
            UserId = user.Id,
            Title = title.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _playlists.AddAsync(playlist);
        await _playlists.SaveChangesAsync();

        TempData["StatusMessage"] = $"Playlist '{playlist.Title}' created!";
        return RedirectToAction(nameof(Playlists));
    }

    // POST /Music/AddToPlaylist
    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToPlaylist(int playlistId, int contentId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Json(new { ok = false, message = "Login required" });

        var playlist = await _playlists.Query()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PlaylistId == playlistId && p.UserId == user.Id);

        if (playlist is null) return Json(new { ok = false, message = "Playlist not found." });

        if (playlist.Items.Any(i => i.ContentId == contentId))
        {
            return Json(new { ok = false, message = "Already in playlist." });
        }

        var item = new PlaylistItem
        {
            PlaylistId = playlistId,
            ContentId = contentId,
            SortOrder = playlist.Items.Count + 1,
            AddedAt = DateTime.UtcNow
        };

        await _playlistItems.AddAsync(item);
        await _playlistItems.SaveChangesAsync();

        return Json(new { ok = true, message = $"Added to '{playlist.Title}'!" });
    }
}
