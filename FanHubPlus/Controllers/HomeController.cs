using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FanHubPlus.Data;
using FanHubPlus.Models;
using FanHubPlus.ViewModels;

namespace FanHubPlus.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly FanHubDbContext _db;

    public HomeController(ILogger<HomeController> logger, FanHubDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _db.Videos.Where(v => v.IsFeatured).ToListAsync(),
            TrendingVideos = await _db.Videos.Where(v => v.IsTrending).ToListAsync(),
            LiveNow = await _db.Videos.Where(v => v.IsLive).ToListAsync(),
            TopRated = await _db.Videos.Where(v => v.IsTopRated).ToListAsync(),
            LatestReleases = await _db.Videos.OrderByDescending(v => v.ReleaseDate).Take(10).ToListAsync(),
            Categories = await _db.Categories.ToListAsync(),
            Testimonials = await _db.Testimonials.ToListAsync(),
            BlogPosts = await _db.BlogPosts.OrderByDescending(b => b.PublishedOn).Take(4).ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
        if (video == null) return NotFound();

        ViewBag.Related = await _db.Videos
            .Where(v => v.Id != id && v.Genre == video.Genre)
            .Take(4)
            .ToListAsync();

        return View(video);
    }

    public async Task<IActionResult> Movies(string? genre, string? search, string? sortBy)
    {
        var query = _db.Videos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(genre) && genre != "All")
        {
            query = query.Where(v => v.Genre.ToLower() == genre.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Title.ToLower().Contains(search.ToLower()) ||
                                     v.Description.ToLower().Contains(search.ToLower()));
        }

        query = sortBy switch
        {
            "rating" => query.OrderByDescending(v => v.ImdbRating),
            "year" => query.OrderByDescending(v => v.Year),
            "title" => query.OrderBy(v => v.Title),
            _ => query.OrderByDescending(v => v.ReleaseDate)
        };

        var allGenres = await _db.Videos.Select(v => v.Genre).Distinct().ToListAsync();

        var model = new MovieFilterViewModel
        {
            Genre = genre,
            Search = search,
            SortBy = sortBy,
            Movies = await query.ToListAsync(),
            Genres = allGenres
        };

        return View(model);
    }

    public IActionResult Pricing()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View(new ContactViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully. Our support team will get back to you shortly.";
            return RedirectToAction(nameof(Contact));
        }
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

