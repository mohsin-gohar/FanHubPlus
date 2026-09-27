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

    // ------------------------------------------------------------------
    // Home pages
    // ------------------------------------------------------------------
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
            BlogPosts = await _db.BlogPosts.OrderByDescending(b => b.PublishedOn).Take(4).ToListAsync(),
            Channels = await _db.Channels.OrderByDescending(c => c.Viewers).Take(6).ToListAsync(),
            StoreDeals = (await _db.Products.ToListAsync())
                .OrderByDescending(p => p.DiscountPercent)
                .Take(4)
                .ToList(),
            Faqs = await _db.Faqs.Where(f => f.IsPopular).Take(4).ToListAsync()
        };

        return View(model);
    }

    /// <summary>Alternate home layout: video-first hero, live channels, editorial picks.</summary>
    public async Task<IActionResult> Showcase()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _db.Videos.Where(v => v.IsFeatured).ToListAsync(),
            TrendingVideos = await _db.Videos.OrderByDescending(v => v.ImdbRating).Take(8).ToListAsync(),
            LiveNow = await _db.Videos.Where(v => v.IsLive).ToListAsync(),
            TopRated = await _db.Videos.Where(v => v.IsTopRated).ToListAsync(),
            LatestReleases = await _db.Videos.OrderByDescending(v => v.ReleaseDate).Take(8).ToListAsync(),
            Categories = await _db.Categories.ToListAsync(),
            Testimonials = await _db.Testimonials.ToListAsync(),
            BlogPosts = await _db.BlogPosts.OrderByDescending(b => b.PublishedOn).Take(3).ToListAsync(),
            Channels = await _db.Channels.ToListAsync(),
            StoreDeals = (await _db.Products.ToListAsync())
                .OrderByDescending(p => p.DiscountPercent)
                .Take(3)
                .ToList(),
            Faqs = await _db.Faqs.Take(4).ToListAsync()
        };

        return View(model);
    }

    // ------------------------------------------------------------------
    // Catalogue
    // ------------------------------------------------------------------
    public async Task<IActionResult> Details(int id)
    {
        var video = await _db.Videos.FirstOrDefaultAsync(v => v.Id == id);
        if (video == null) return NotFound();

        var model = new DetailsViewModel
        {
            Title = video,
            Episodes = await _db.Episodes.Where(e => e.VideoId == id)
                .OrderBy(e => e.Season).ThenBy(e => e.Number).ToListAsync(),
            Related = await _db.Videos
                .Where(v => v.Id != id && (v.Genre == video.Genre || v.IsTopRated))
                .Take(6).ToListAsync(),
            Comments = await _db.Comments.Where(c => c.VideoId == id && c.ParentId == null)
                .OrderByDescending(c => c.PostedOn).ToListAsync(),
            Replies = await _db.Comments.Where(c => c.VideoId == id && c.ParentId != null).ToListAsync()
        };
        model.WatchlistCount = Math.Max(0, await _db.Videos.CountAsync() - 1);
        model.AverageScore = video.ImdbRating > 0 ? (int)Math.Round(video.ImdbRating * 10) : 86;

        return View(model);
    }

    public Task<IActionResult> Movies(string? genre, string? search, string? sortBy, int page = 1)
        => CatalogueAsync("Movies", genre, search, sortBy, page);

    public Task<IActionResult> TvShows(string? genre, string? search, string? sortBy, int page = 1)
        => CatalogueAsync("TV Shows", genre, search, sortBy, page);

    public Task<IActionResult> Videos(string? genre, string? search, string? sortBy, int page = 1)
        => CatalogueAsync("Videos", genre, search, sortBy, page);

    private async Task<IActionResult> CatalogueAsync(string section, string? genre, string? search, string? sortBy, int page)
    {
        var query = _db.Videos.AsQueryable();

        if (section == "TV Shows") query = query.Where(v => v.IsSeries);
        if (section == "Videos") query = query.Where(v => v.IsLive);

        if (!string.IsNullOrWhiteSpace(genre) && genre != "All")
        {
            query = query.Where(v => v.Genre.ToLower() == genre.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(v => v.Title.ToLower().Contains(term)
                || v.Description.ToLower().Contains(term)
                || v.Genre.ToLower().Contains(term)
                || v.Cast.ToLower().Contains(term));
        }

        var total = await query.CountAsync();

        query = sortBy switch
        {
            "rating" => query.OrderByDescending(v => v.ImdbRating),
            "year" => query.OrderByDescending(v => v.Year),
            "title" => query.OrderBy(v => v.Title),
            "oldest" => query.OrderBy(v => v.ReleaseDate),
            _ => query.OrderByDescending(v => v.ReleaseDate)
        };

        var pageSize = 12;
        var current = Math.Max(1, page);

        var model = new BrowseViewModel
        {
            Section = section,
            Genre = genre,
            Search = search,
            SortBy = sortBy,
            Page = current,
            PageSize = pageSize,
            TotalCount = total,
            Items = await query.Skip(BrowseViewModel.SkipTake(current, pageSize)).Take(pageSize).ToListAsync(),
            Categories = await _db.Categories.ToListAsync(),
            Genres = await _db.Videos.Select(v => v.Genre).Distinct().OrderBy(g => g).ToListAsync()
        };

        // Movies / TV Shows / Live Videos all render the same catalogue layout.
        return View("Movies", model);
    }

    public async Task<IActionResult> Categories()
    {
        var model = new BrowseViewModel
        {
            Section = "Categories",
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
        };

        foreach (var category in model.Categories)
        {
            category.VideoCount = await _db.Videos.CountAsync(v => v.Genre == category.Name);
        }

        return View(model);
    }

    public async Task<IActionResult> Channels(string? category)
    {
        var query = _db.Channels.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(c => c.Category == category);
        }

        var model = new BrowseViewModel
        {
            Section = "Channels",
            Channels = await query.OrderByDescending(c => c.Viewers).ToListAsync(),
            Genres = await _db.Channels.Select(c => c.Category).Distinct().OrderBy(c => c).ToListAsync()
        };

        return View(model);
    }

    // ------------------------------------------------------------------
    // Company + editorial pages
    // ------------------------------------------------------------------
    public IActionResult About()
    {
        ViewBag.Stats = new[]
        {
            new { Value = 12400d, Label = "Titles in the library" },
            new { Value = 190d, Label = "Countries reached" },
            new { Value = 42d, Label = "Original productions" },
            new { Value = 8.4d, Label = "Average viewer score" }
        };
        ViewBag.Milestones = new[]
        {
            new { Year = "2019", Title = "A single shared drive", Body = "FanHub Plus started as a hand-curated folder of films shared between friends in Mumbai." },
            new { Year = "2021", Title = "First app, first original", Body = "We shipped iOS, Android and TV apps and released our first original series to subscribers." },
            new { Year = "2023", Title = "Live sport at scale", Body = "Low latency live channels brought cricket, football and concerts to the same login as the library." },
            new { Year = "2025", Title = "4K HDR everywhere", Body = "Dolby Vision, Atmos and offline downloads rolled out across every supported device." }
        };
        ViewBag.Team = _db.Testimonials.ToList();
        return View();
    }

    public IActionResult Services() => View();

    public IActionResult ServiceDetails(int id)
    {
        var services = new[]
        {
            new { Id = 1, Slug = "streaming", Title = "FanHub Streaming", Icon = "ri-play-circle-line", Theme = "t1", Lead = "Unlimited ad-free streaming on tens of thousands of screens a day.", Body = "Our core subscription puts the whole library - films, series, anime, documentaries and live sport - behind one login. Adaptive bitrate switching keeps playback steady on a two-bar phone connection and still finds headroom for 4K HDR on a television.", Points = new[] { "Up to four simultaneous screens on Premium 4K UHD", "Offline downloads for thirty days on mobile and laptop", "Five extra profiles including a PIN protected kids profile", "Audio in up to four languages with subtitles in five" }, Cta = "See plans" },
            new { Id = 2, Slug = "live", Title = "Live Channels & Sport", Icon = "ri-broadcast-line", Theme = "t4", Lead = "Low latency live sport, news and events with instant replays.", Body = "FanHub carries live channels for cricket, football, news, kids and music. Streams start in under two seconds, carry an optional scorecard overlay and are archived so you can jump back to any moment of a match.", Points = new[] { "Five to eight second delay over the internet", "Instant replay from the start of every fixture", "Match centre with stats, lineups and highlights", "Channel packs that bundle sport, news and kids" }, Cta = "Browse channels" },
            new { Id = 3, Slug = "store", Title = "FanHub Store", Icon = "ri-shopping-bag-3-line", Theme = "t6", Lead = "Official merch, soundtracks and hardware for the things you love.", Body = "The store carries limited edition apparel, vinyl and Blu-Ray editions, posters and streaming hardware. Every item ships from the same warehouse as our devices, and members get early access to drops.", Points = new[] { "Members only pricing on selected drops", "Tracked delivery across India", "Thirty day returns on physical goods", "Gift wrapping at checkout" }, Cta = "Visit the store" },
            new { Id = 4, Slug = "partners", Title = "Studio Partnerships", Icon = "ri-building-2-line", Theme = "t5", Lead = "Distribution and production partnerships with studios worldwide.", Body = "FanHub works with independent studios, national broadcasters and festival distributors to bring licensed titles to new audiences. Our partner team handles catalogue ingest, artwork, subtitles and reporting.", Points = new[] { "Catalogue ingest in days, not months", "Automated subtitle and dubbing pipelines", "Monthly viewing and revenue reporting", "Co-marketing budgets for priority titles" }, Cta = "Talk to us" }
        };

        var service = services.FirstOrDefault(s => s.Id == id || s.Slug == id.ToString());
        if (service is null) return NotFound();

        ViewBag.Related = services.Where(s => s.Id != service.Id).ToArray();
        return View(service);
    }

    public IActionResult Devices() => View();

    public async Task<IActionResult> Careers()
    {
        var jobs = await _db.Jobs.OrderByDescending(j => j.PostedOn).ToListAsync();
        return View(jobs);
    }

    public async Task<IActionResult> CareerDetails(int id)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();
        ViewBag.Others = await _db.Jobs.Where(j => j.Id != id).OrderByDescending(j => j.PostedOn).Take(4).ToListAsync();
        return View(job);
    }

    public async Task<IActionResult> Blog(string layout = "right", string? category = null, int page = 1)
    {
        var query = _db.BlogPosts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            query = query.Where(b => b.Category == category);
        }

        var pageSize = 6;
        var current = Math.Max(1, page);
        var all = await _db.BlogPosts.ToListAsync();

        var model = new BlogListViewModel
        {
            Layout = layout,
            Category = category,
            Page = current,
            PageSize = pageSize,
            TotalCount = await query.CountAsync(),
            Posts = await query.OrderByDescending(b => b.PublishedOn)
                .Skip(BrowseViewModel.SkipTake(current, pageSize)).Take(pageSize).ToListAsync(),
            Latest = all.OrderByDescending(b => b.PublishedOn).Take(4).ToList(),
            Popular = all.OrderByDescending(b => b.Comments).Take(4).ToList(),
            Categories = all.Select(b => b.Category).Distinct().OrderBy(c => c).ToList(),
            Tags = all.SelectMany(b => b.TagList)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(14)
                .ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> BlogDetails(int id, string sidebar = "right")
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);
        if (post == null) return NotFound();

        var model = new BlogDetailsViewModel
        {
            Post = post,
            Sidebar = sidebar,
            Related = await _db.BlogPosts.Where(b => b.Id != id)
                .OrderBy(b => b.Category == post.Category ? 0 : 1)
                .ThenByDescending(b => b.PublishedOn)
                .Take(3).ToListAsync(),
            Latest = await _db.BlogPosts.OrderByDescending(b => b.PublishedOn).Take(4).ToListAsync(),
            Popular = await _db.BlogPosts.OrderByDescending(b => b.Comments).Take(4).ToListAsync(),
            Comments = await _db.Comments.Where(c => c.VideoId == 0).OrderByDescending(c => c.PostedOn).ToListAsync()
        };

        return View(model);
    }


    public async Task<IActionResult> Faq(string? topic)
    {
        var query = _db.Faqs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(topic) && topic != "All")
        {
            query = query.Where(f => f.Topic == topic);
        }

        ViewBag.Topics = await _db.Faqs.Select(f => f.Topic).Distinct().OrderBy(t => t).ToListAsync();
        ViewBag.ActiveTopic = topic ?? "All";
        return View(await query.OrderBy(f => f.Id).ToListAsync());
    }

    /// <summary>Second home layout: video-first hero, channel line-up and editorial picks.</summary>
    public async Task<IActionResult> IndexTwo()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _db.Videos.Where(v => v.IsFeatured).ToListAsync(),
            TrendingVideos = await _db.Videos.OrderByDescending(v => v.ImdbRating).Take(8).ToListAsync(),
            LiveNow = await _db.Videos.Where(v => v.IsLive).ToListAsync(),
            TopRated = await _db.Videos.Where(v => v.IsTopRated).ToListAsync(),
            LatestReleases = await _db.Videos.OrderByDescending(v => v.ReleaseDate).Take(8).ToListAsync(),
            Categories = await _db.Categories.ToListAsync(),
            Testimonials = await _db.Testimonials.ToListAsync(),
            BlogPosts = await _db.BlogPosts.OrderByDescending(b => b.PublishedOn).Take(3).ToListAsync(),
            Channels = await _db.Channels.ToListAsync(),
            StoreDeals = (await _db.Products.ToListAsync())
                .OrderByDescending(p => p.DiscountPercent)
                .Take(3)
                .ToList(),
            Faqs = await _db.Faqs.Take(4).ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Testimonials()
    {
        ViewBag.Stats = new[]
        {
            new { Value = 12400d, Label = "Titles in the library" },
            new { Value = 190d, Label = "Countries reached" },
            new { Value = 42d, Label = "Original productions" },
            new { Value = 8.4d, Label = "Average viewer score" }
        };
        return View(await _db.Testimonials.ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Newsletter(string email)
    {
        TempData["SuccessMessage"] = string.IsNullOrWhiteSpace(email)
            ? "Enter an email address and we will add you to the list."
            : $"Thanks - release alerts are now going to {email}.";
        return RedirectToAction(nameof(Index));
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

    public IActionResult Terms()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        Response.StatusCode = statusCode is 404 or 500 ? statusCode.Value : StatusCodes.Status500InternalServerError;

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = Response.StatusCode,
            Title = Response.StatusCode == 404 ? "Page not found" : "Something went wrong"
        });
    }
}

