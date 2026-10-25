using FanHubPlus.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FanHubPlus.Controllers;

/// <summary>
/// YouTube video gallery - lets visitors watch YouTube videos and songs
/// directly on FanHubPlus, just like YouTube embeds on other websites.
/// </summary>
public class YouTubeVideoController : Controller
{
    private static readonly List<YouTubeVideoItem> _featuredVideos = new();

    public YouTubeVideoController()
    {
        _featuredVideos.AddRange(new[]
        {
            new YouTubeVideoItem
            {
                VideoId = "dQw4w9WgXcQ",
                Title = "Rick Astley - Never Gonna Give You Up (Official Music Video)",
                Category = "Music",
                Description = "Classic 80s hit music video",
                ThumbnailUrl = "https://img.youtube.com/vi/dQw4w9WgXcQ/maxresdefault.jpg",
                Duration = "3:33",
                Views = "1.2B",
                UploadedBy = "Rick Astley"
            },
            new YouTubeVideoItem
            {
                VideoId = "kffacxfA7G4",
                Title = "Beyoncé - BREAK MY SOUL (Official Music Video)",
                Category = "Music",
                Description = "Break My Soul - Official music video by Beyoncé",
                ThumbnailUrl = "https://img.youtube.com/vi/kffacxfA7G4/maxresdefault.jpg",
                Duration = "4:29",
                Views = "85M",
                UploadedBy = "Beyoncé"
            },
            new YouTubeVideoItem
            {
                VideoId = "yQ6sSap9bBw",
                Title = "Trailer - Latest Blockbuster Movie (Official Trailer)",
                Category = "Movies",
                Description = "Official trailer for the upcoming blockbuster film",
                ThumbnailUrl = "https://img.youtube.com/vi/yQ6sSap9bBw/maxresdefault.jpg",
                Duration = "2:15",
                Views = "45M",
                UploadedBy = "Movie Studios"
            },
            new YouTubeVideoItem
            {
                VideoId = "9bZkp7q19f0",
                Title = "PSY - GANGNAM STYLE (Official Music Video)",
                Category = "Music",
                Description = "Gangnam Style - Global hit music video",
                ThumbnailUrl = "https://img.youtube.com/vi/9bZkp7q19f0/maxresdefault.jpg",
                Duration = "4:13",
                Views = "4.2B",
                UploadedBy = "PSY Official"
            },
            new YouTubeVideoItem
            {
                VideoId = "jNQXAC9IVRw",
                Title = "First YouTube Video - Me at the zoo",
                Category = "Viral",
                Description = "The very first video ever uploaded to YouTube",
                ThumbnailUrl = "https://img.youtube.com/vi/jNQXAC9IVRw/maxresdefault.jpg",
                Duration = "0:19",
                Views = "230M",
                UploadedBy = "jawed"
            },
            new YouTubeVideoItem
            {
                VideoId = "CevxZvSJLk8",
                Title = "Katy Perry - Roar (Official Music Video)",
                Category = "Music",
                Description = "Roar - Official music video by Katy Perry",
                ThumbnailUrl = "https://img.youtube.com/vi/CevxZvSJLk8/maxresdefault.jpg",
                Duration = "4:32",
                Views = "3.8B",
                UploadedBy = "Katy Perry"
            }
        });
    }

    // GET /YouTubeVideo - Video gallery page
    public IActionResult Index(string? category, string? search, int page = 1)
    {
        var videos = _featuredVideos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            videos = videos.Where(v => v.Category!.Equals(category, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            videos = videos.Where(v =>
                v.Title.ToLowerInvariant().Contains(s) ||
                v.Description.ToLowerInvariant().Contains(s) ||
                v.UploadedBy.ToLowerInvariant().Contains(s));
        }

        var total = videos.Count();
        var pageSize = 12;
        var totalPages = (int)Math.Ceiling((double)total / pageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        var vm = new YouTubeVideoGalleryViewModel
        {
            Videos = videos.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Categories = _featuredVideos.Select(v => v.Category).Distinct().OrderBy(c => c).ToList(),
            SelectedCategory = category,
            Search = search,
            Page = page,
            TotalPages = totalPages,
            TotalItems = total
        };

        return View(vm);
    }

    // GET /YouTubeVideo/Watch/{videoId} - Watch a specific YouTube video
    public IActionResult Watch(string videoId)
    {
        if (string.IsNullOrWhiteSpace(videoId))
            return NotFound();

        var video = _featuredVideos.FirstOrDefault(v => v.VideoId == videoId);
        if (video is null)
            return NotFound();

        var related = _featuredVideos
            .Where(v => v.VideoId != videoId)
            .Take(8)
            .ToList();

        var vm = new YouTubeVideoWatchViewModel
        {
            Video = video,
            RelatedVideos = related
        };

        return View(vm);
    }
}