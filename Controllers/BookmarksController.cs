using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FanHubPlus.Controllers;

// My Bookmarks (grouped by item type) + AJAX toggle/remove used by every detail page
[Authorize]
public class BookmarksController : Controller
{
    private readonly ISupportService _support;
    private readonly IBookmarkService _bookmarks;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookmarksController(ISupportService support,
                               IBookmarkService bookmarks,
                               UserManager<ApplicationUser> userManager)
    {
        _support = support;
        _bookmarks = bookmarks;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");
        return View(await _support.GetBookmarksAsync(user.Id));
    }

    // POST /Bookmarks/Toggle (AJAX JSON) - body: type=<BookmarkType int>&itemId=5
    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("write")]
    public async Task<IActionResult> Toggle(int type, int itemId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Json(new { ok = false, auth = false, message = "Please log in to bookmark." });

        if (!Enum.IsDefined(typeof(BookmarkType), type))
            return Json(new { ok = false, message = "Invalid bookmark type." });

        if (itemId <= 0)
            return Json(new { ok = false, message = "Invalid item." });

        try
        {
            var bookmarked = await _bookmarks.ToggleAsync(user.Id, (BookmarkType)type, itemId);
            return Json(new
            {
                ok = true,
                bookmarked,
                message = bookmarked ? "Added to your bookmarks." : "Removed from your bookmarks."
            });
        }
        catch (KeyNotFoundException ex)
        {
            // Target row is gone (e.g. an admin deleted it) - never claim success
            return Json(new { ok = false, message = ex.Message });
        }
    }

    // POST /Bookmarks/Remove/12 (classic form post from My Bookmarks page)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        // Direct, user-scoped delete (the old code loaded every bookmark + an N+1
        // lookup and reported success even when nothing was removed).
        var removed = await _bookmarks.RemoveAsync(user.Id, id);

        TempData[removed ? "StatusMessage" : "StatusError"] =
            removed ? "Bookmark removed." : "That bookmark could not be found (it may already be removed).";

        return RedirectToAction(nameof(Index));
    }
}
