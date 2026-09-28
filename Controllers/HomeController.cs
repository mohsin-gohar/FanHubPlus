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

    public HomeController(IStatsService stats,
                          IContentService content,
                          IRepository<FanHubPlus.Models.Entities.Category> categories)
    {
        _stats = stats;
        _content = content;
        _categories = categories;
    }

    // Landing page: hero counters + trending content + latest news + upcoming events + merch
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> Index()
    {
        // Fire all independent DB queries concurrently instead of sequentially
        var totalsTask = _stats.GetPortalTotalsAsync();
        var categoriesTask = _categories.Query()
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
        var trendingTask = _content.GetTrendingAsync(6);
        var trailersTask = _content.GetTrailersAsync(6);
        var articlesTask = _content.GetLatestArticlesAsync(3);
        var eventsTask = _content.GetUpcomingEventsAsync(3);
        var merchTask = _content.GetFeaturedMerchAsync(4);

        await Task.WhenAll(
            totalsTask, categoriesTask, trendingTask,
            trailersTask, articlesTask, eventsTask, merchTask);

        var (contents, members, events) = await totalsTask;

        var vm = new HomeViewModel
        {
            Categories = await categoriesTask,
            Trending = await trendingTask,
            Trailers = await trailersTask,
            LatestArticles = await articlesTask,
            UpcomingEvents = await eventsTask,
            FeaturedMerch = await merchTask,
            TotalContents = contents,
            TotalMembers = members,
            TotalEvents = events
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
