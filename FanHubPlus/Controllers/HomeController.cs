using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.ViewModels;
using FanHubPlus.Services.Interfaces;
using FanHubPlus.DTOs;

namespace FanHubPlus.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IVideoService _videoService;
    private readonly IBlogService _blogService;
    private readonly IStoreService _storeService;
    private readonly IChannelService _channelService;
    private readonly ITestimonialService _testimonialService;
    private readonly IFaqService _faqService;
    private readonly ICareerService _careerService;

    public HomeController(
        ILogger<HomeController> logger,
        IVideoService videoService,
        IBlogService blogService,
        IStoreService storeService,
        IChannelService channelService,
        ITestimonialService testimonialService,
        IFaqService faqService,
        ICareerService careerService)
    {
        _logger = logger;
        _videoService = videoService;
        _blogService = blogService;
        _storeService = storeService;
        _channelService = channelService;
        _testimonialService = testimonialService;
        _faqService = faqService;
        _careerService = careerService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _videoService.GetFeaturedAsync(3),
            TrendingVideos = await _videoService.GetTrendingAsync(8),
            LiveNow = await _videoService.GetLiveAsync(6),
            TopRated = await _videoService.GetTopRatedAsync(5),
            LatestReleases = await _videoService.GetLatestAsync(10),
            Categories = await _videoService.GetCategoriesAsync(),
            Testimonials = await _testimonialService.GetAllAsync(),
            BlogPosts = await _blogService.GetLatestAsync(4),
            Channels = await _channelService.GetTopChannelsAsync(6),
            StoreDeals = await _storeService.GetFeaturedAsync(4),
            Faqs = await _faqService.GetPopularAsync(4)
        };

        return View(model);
    }

    public async Task<IActionResult> Showcase()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _videoService.GetFeaturedAsync(3),
            TrendingVideos = await _videoService.GetTrendingAsync(8),
            LiveNow = await _videoService.GetLiveAsync(6),
            TopRated = await _videoService.GetTopRatedAsync(5),
            LatestReleases = await _videoService.GetLatestAsync(8),
            Categories = await _videoService.GetCategoriesAsync(),
            Testimonials = await _testimonialService.GetAllAsync(),
            BlogPosts = await _blogService.GetLatestAsync(3),
            Channels = await _channelService.GetTopChannelsAsync(6),
            StoreDeals = await _storeService.GetFeaturedAsync(3),
            Faqs = await _faqService.GetPopularAsync(4)
        };

        return View(model);
    }

    public async Task<IActionResult> IndexTwo()
    {
        var model = new HomeIndexViewModel
        {
            FeaturedHero = await _videoService.GetFeaturedAsync(3),
            TrendingVideos = await _videoService.GetTrendingAsync(8),
            LiveNow = await _videoService.GetLiveAsync(6),
            TopRated = await _videoService.GetTopRatedAsync(5),
            LatestReleases = await _videoService.GetLatestAsync(8),
            Categories = await _videoService.GetCategoriesAsync(),
            Testimonials = await _testimonialService.GetAllAsync(),
            BlogPosts = await _blogService.GetLatestAsync(3),
            Channels = await _channelService.GetTopChannelsAsync(6),
            StoreDeals = await _storeService.GetFeaturedAsync(3),
            Faqs = await _faqService.GetPopularAsync(4)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _videoService.GetDetailAsync(id);
        if (detail == null) return NotFound();

        var model = new DetailsViewModel
        {
            Title = detail.Video,
            Episodes = detail.Episodes,
            Related = detail.Related,
            Comments = detail.Comments,
            Replies = detail.Replies,
            WatchlistCount = Math.Max(0, (await _videoService.GetPagedAsync(new VideoFilter())).TotalCount - 1),
            AverageScore = detail.Video.ImdbRating > 0 ? (int)Math.Round(detail.Video.ImdbRating * 10) : 86
        };

        return View(model);
    }

    public async Task<IActionResult> Movies(string? genre, string? search, string? sortBy, int page = 1)
        => await CatalogueAsync(VideoType.Movies, genre, search, sortBy, page);

    public async Task<IActionResult> TvShows(string? genre, string? search, string? sortBy, int page = 1)
        => await CatalogueAsync(VideoType.TvShows, genre, search, sortBy, page);

    public async Task<IActionResult> Videos(string? genre, string? search, string? sortBy, int page = 1)
        => await CatalogueAsync(VideoType.Live, genre, search, sortBy, page);

    private async Task<IActionResult> CatalogueAsync(VideoType section, string? genre, string? search, string? sortBy, int page)
    {
        var filter = new VideoFilter
        {
            Type = section,
            Genre = genre,
            Search = search,
            SortBy = sortBy ?? "latest",
            Page = Math.Max(1, page)
        };

        var result = await _videoService.GetPagedAsync(filter);

        var model = new BrowseViewModel
        {
            Section = section.ToString(),
            Genre = genre,
            Search = search,
            SortBy = sortBy,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items,
            Categories = await _videoService.GetCategoriesAsync(),
            Genres = (await _videoService.GetPagedAsync(new VideoFilter { PageSize = 1000 })).Items.Select(v => v.Genre).Distinct().OrderBy(g => g).ToList()
        };

        return View("Movies", model);
    }

    public async Task<IActionResult> Categories()
    {
        var categories = await _videoService.GetCategoriesAsync();
        foreach (var category in categories)
        {
            var videos = await _videoService.GetByGenreAsync(category.Name, 1);
            category.VideoCount = videos.Count > 0 ? videos.Count : 0;
        }

        var model = new BrowseViewModel
        {
            Section = "Categories",
            Categories = categories
        };

        return View(model);
    }

    public async Task<IActionResult> Channels(string? category)
    {
        var model = new BrowseViewModel
        {
            Section = "Channels",
            Channels = await _channelService.GetChannelsAsync(category),
            Genres = await _channelService.GetCategoriesAsync()
        };

        return View(model);
    }

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
        var jobs = await _careerService.GetAllAsync();
        return View(jobs);
    }

    public async Task<IActionResult> CareerDetails(int id)
    {
        var job = await _careerService.GetByIdAsync(id);
        if (job == null) return NotFound();
        ViewBag.Others = await _careerService.GetOtherJobsAsync(id);
        return View(job);
    }

    public async Task<IActionResult> Blog(string layout = "right", string? category = null, int page = 1)
    {
        var filter = new BlogFilter
        {
            Category = category,
            Layout = layout,
            Page = Math.Max(1, page)
        };

        var result = await _blogService.GetPagedAsync(filter);

        var model = new BlogListViewModel
        {
            Layout = layout,
            Category = category,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Posts = result.Items,
            Latest = result.AdditionalData?.Latest ?? new(),
            Popular = result.AdditionalData?.Popular ?? new(),
            Categories = result.AdditionalData?.Categories ?? new(),
            Tags = result.AdditionalData?.Tags ?? new()
        };

        return View(model);
    }

    public async Task<IActionResult> BlogDetails(int id, string sidebar = "right")
    {
        var detail = await _blogService.GetDetailAsync(id);
        if (detail == null) return NotFound();

        var model = new BlogDetailsViewModel
        {
            Post = detail.Post,
            Sidebar = sidebar,
            Related = detail.Related,
            Latest = detail.Latest,
            Popular = detail.Popular,
            Comments = detail.Comments
        };

        return View(model);
    }

    public async Task<IActionResult> Faq(string? topic)
    {
        var query = await _faqService.GetAllAsync(topic);
        ViewBag.Topics = await _faqService.GetTopicsAsync();
        ViewBag.ActiveTopic = topic ?? "All";
        return View(query);
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
        return View(await _testimonialService.GetAllAsync());
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

    public IActionResult Pricing() => View();

    public IActionResult Contact() => View(new ContactViewModel());

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

    public IActionResult Privacy() => View();
    public IActionResult Terms() => View();

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