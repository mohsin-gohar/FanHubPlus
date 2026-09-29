using System.Diagnostics;
using FanHubPlus.Models;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers;

public class HomeController : Controller
{
    private readonly IStatsService _stats;
    private readonly IContentService _content;
    private readonly IRepository<FanHubPlus.Models.Entities.Category> _categories;
    private readonly IRepository<FanHubPlus.Models.Entities.Content> _contents;

    public HomeController(IStatsService stats,
                          IContentService content,
                          IRepository<FanHubPlus.Models.Entities.Category> categories,
                          IRepository<FanHubPlus.Models.Entities.Content> contents)
    {
        _stats = stats;
        _content = content;
        _categories = categories;
        _contents = contents;
    }

    // Landing page: hero counters + trending content + latest news + upcoming events + merch
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> Index()
    {
        // Every service in this request shares ONE scoped ApplicationDbContext
        // (Unit of Work), which EF Core does NOT allow to run operations on
        // concurrently - firing these with Task.WhenAll threw
        // "A second operation was started on this context instance" and 500'd
        // the landing page. The queries are cheap (counts + small Take() lists)
        // and the response is cached for 5 minutes, so they run sequentially.
        var totals = await _stats.GetPortalTotalsAsync();
        var categories = await _categories.Query()
            .Select(c => new CategorySummaryViewModel
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                IconUrl = c.IconUrl,
                Description = c.Description,
                ContentCount = c.Contents.Count
            })
            .OrderBy(c => c.Name)
            .ToListAsync();
        var trending = await _content.GetTrendingAsync(6);
        var trailers = await _content.GetTrailersAsync(6);
        var articles = await _content.GetLatestArticlesAsync(3);
        var events = await _content.GetUpcomingEventsAsync(3);
        var merch = await _content.GetFeaturedMerchAsync(4);
        var featuredMovies = await _contents.Query().AsNoTracking()
            .Include(c => c.Category)
            .Where(c => c.Type == FanHubPlus.Models.Enums.ContentType.Movie
                     || c.Type == FanHubPlus.Models.Enums.ContentType.Series
                     || c.Type == FanHubPlus.Models.Enums.ContentType.Documentary)
            .OrderByDescending(c => c.PopularityScore)
            .Take(8)
            .ToListAsync();
        var featuredMusic = await _contents.Query().AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => c.Type == FanHubPlus.Models.Enums.ContentType.Song
                     || c.Type == FanHubPlus.Models.Enums.ContentType.Album)
            .OrderByDescending(c => c.PopularityScore)
            .Take(8)
            .ToListAsync();
        var featuredGames = await _contents.Query().AsNoTracking()
            .Include(c => c.Category)
            .Where(c => c.Type == FanHubPlus.Models.Enums.ContentType.Game
                     || c.Type == FanHubPlus.Models.Enums.ContentType.PlayableGame)
            .OrderByDescending(c => c.PopularityScore)
            .Take(8)
            .ToListAsync();

        var (contents, members, upcoming) = totals;

        var vm = new HomeViewModel
        {
            Categories = categories,
            Trending = trending,
            Trailers = trailers,
            LatestArticles = articles,
            UpcomingEvents = events,
            FeaturedMerch = merch,
            FeaturedMovies = featuredMovies,
            FeaturedMusic = featuredMusic,
            FeaturedGames = featuredGames,
            TotalContents = contents,
            TotalMembers = members,
            TotalEvents = upcoming
        };

        return View(vm);
    }

    // Shared error page (exception handler + 404/500 status-code re-execution)
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        ViewBag.StatusCode = statusCode;
        ViewBag.ErrorPath = feature?.Path;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
