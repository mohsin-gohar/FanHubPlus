using FanHubPlus.Models.ViewModels;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.Enums;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Content domain logic shared by Home, Explorer, detail pages and the Rating AJAX endpoint.
/// </summary>
public class ContentService : IContentService
{
    private readonly IRepository<Content> _contents;
    private readonly IRepository<Tag> _tags;
    private readonly IRepository<ContentTag> _contentTags;
    private readonly IRepository<Rating> _ratings;
    private readonly IRepository<MerchandiseItem> _merch;
    private readonly IRepository<Article> _articles;
    private readonly IRepository<EventItem> _events;

    public ContentService(IRepository<Content> contents,
                          IRepository<Tag> tags,
                          IRepository<ContentTag> contentTags,
                          IRepository<Rating> ratings,
                          IRepository<MerchandiseItem> merch,
                          IRepository<Article> articles,
                          IRepository<EventItem> events)
    {
        _contents = contents;
        _tags = tags;
        _contentTags = contentTags;
        _ratings = ratings;
        _merch = merch;
        _articles = articles;
        _events = events;
    }

    // ---- Tag synchronisation: "ninja,space" -> ensure Tag rows exist, replace ContentTags ----
    public async Task SyncTagsAsync(Content content, string? rawTags)
    {
        var desired = (rawTags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .ToList();

        var existingLinks = await _contentTags.Query()
            .Where(ct => ct.ContentId == content.ContentId)
            .Include(ct => ct.Tag)
            .ToListAsync();

        // Remove links whose tag is no longer requested
        foreach (var link in existingLinks.Where(l => !desired.Contains(l.Tag.Name.ToLowerInvariant())))
            _contentTags.Remove(link);

        var linkedNames = existingLinks
            .Select(l => l.Tag.Name.ToLowerInvariant())
            .ToHashSet();

        var needed = desired.Where(d => !linkedNames.Contains(d)).ToList();

        if (needed.Count > 0)
        {
            // ONE query for the whole (small) tag table instead of one query per tag
            var allTags = await _tags.Query().AsNoTracking().ToListAsync();
            var byName = allTags.ToDictionary(t => t.Name.ToLowerInvariant(), t => t);

            foreach (var name in needed)
            {
                if (!byName.TryGetValue(name, out var tag))
                {
                    tag = new Tag { Name = name };
                    _tags.Add(tag);            // inserted together with the links below
                    byName[name] = tag;
                }

                await _contentTags.AddAsync(new ContentTag { Content = content, Tag = tag });
            }
        }

        // Single flush for tag inserts + link changes (was up to 3 round-trips)
        await _contentTags.SaveChangesAsync();
    }

    public Task<List<Content>> GetTrendingAsync(int count)
        => _contents.Query()
            .OrderByDescending(c => c.PopularityScore)
            .ThenByDescending(c => c.ViewCount)
            .Take(count)
            .ToListAsync();

    public Task<List<MerchandiseItem>> GetFeaturedMerchAsync(int count)
        => _merch.Query()
            .Where(m => !m.IsUpcoming)
            .OrderByDescending(m => m.ViewCount)
            .Take(count)
            .ToListAsync();

    public Task<List<Article>> GetLatestArticlesAsync(int count)
        => _articles.Query()
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .Take(count)
            .ToListAsync();

    public Task<List<EventItem>> GetUpcomingEventsAsync(int count)
        => _events.Query()
            .Where(e => e.EventDate >= DateTime.UtcNow)
            .OrderBy(e => e.EventDate)
            .Take(count)
            .ToListAsync();

    // ---- Trailers for the landing page: only titles that really have a video ----
    public async Task<List<TrailerViewModel>> GetTrailersAsync(int count)
    {
        var rows = await _contents.Query()
            .Include(c => c.Category)
            .Include(c => c.MediaItems)
            .Where(c => c.MediaItems.Any(m => m.MediaType == MediaType.Video || m.MediaType == MediaType.Trailer))
            .OrderByDescending(c => c.PopularityScore)
            .ThenByDescending(c => c.ViewCount)
            .ToListAsync();

        var trailers = new List<TrailerViewModel>();
        foreach (var content in rows)
        {
            // A row with no URL is not playable, so skip it instead of showing a dead card
            var media = MediaUrl.Primary(content.MediaItems);
            if (media is null) continue;

            trailers.Add(new TrailerViewModel
            {
                Content = content,
                Media = media,
                Url = media.EmbedUrl,
                Kind = MediaUrl.Kind(media.EmbedUrl),
                // The video poster is a better "trailer still" than the card cover
                Poster = string.IsNullOrWhiteSpace(content.ThumbnailUrl)
                    ? $"/assets/images/videos/video{(content.ContentId % 34) + 1}.jpg"
                    : content.ThumbnailUrl,
                Label = string.IsNullOrWhiteSpace(media.Tag) ? "Trailer" : media.Tag
            });

            if (trailers.Count == count) break;
        }

        return trailers;
    }

    // ---- Rating upsert (unique user+content enforced by DB + service logic) ----
    public async Task<(double avg, int count, int myStars)> RateAsync(string userId, int contentId, int stars)
    {
        if (stars is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(stars));

        // The target must exist, otherwise the FK insert would surface as a 500.
        var contentExists = await _contents.Query().AsNoTracking().AnyAsync(c => c.ContentId == contentId);
        if (!contentExists) throw new KeyNotFoundException($"Content {contentId} no longer exists.");

        var existing = await _ratings.Query()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ContentId == contentId);

        if (existing is null)
        {
            await _ratings.AddAsync(new Rating { UserId = userId, ContentId = contentId, Stars = stars });

            try
            {
                await _ratings.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two requests raced: the unique index (UserId, ContentId) rejected the
                // second insert. Fall back to updating the row that won instead of 500.
                var winner = await _ratings.Query()
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ContentId == contentId);
                if (winner is null) throw;

                winner.Stars = stars;
                winner.CreatedAt = DateTime.UtcNow;
                await _ratings.SaveChangesAsync();
            }
        }
        else
        {
            existing.Stars = stars;
            existing.CreatedAt = DateTime.UtcNow;
            await _ratings.SaveChangesAsync();
        }

        // Aggregate in SQL - the old code loaded every rating row into memory
        var count = await _ratings.Query().AsNoTracking()
            .CountAsync(r => r.ContentId == contentId);

        var avg = count == 0
            ? 0
            : Math.Round(await _ratings.Query().AsNoTracking()
                .Where(r => r.ContentId == contentId)
                .AverageAsync(r => (double)r.Stars), 1);

        return (avg, count, stars);
    }
}


