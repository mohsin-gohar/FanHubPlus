using FanHubPlus.Application.Interfaces;
using FanHubPlus.Core.Entities;
using FanHubPlus.Core.Enums;
using FanHubPlus.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Application.Services;

public class ContentService : IContentService
{
    private readonly IRepository<Content> _contentRepository;
    private readonly IRepository<Rating> _ratingRepository;

    public ContentService(IRepository<Content> contentRepository, IRepository<Rating> ratingRepository)
    {
        _contentRepository = contentRepository;
        _ratingRepository = ratingRepository;
    }

    public Task<Content?> GetContentByIdAsync(int contentId, CancellationToken ct = default)
        => _contentRepository.QueryNoTracking().FirstOrDefaultAsync(c => c.ContentId == contentId, ct);

    public async Task<List<Content>> GetContentsByCategoryAsync(
        int categoryId, ContentType? type, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _contentRepository.QueryNoTracking().Where(c => c.CategoryId == categoryId);
        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        return await query
            .OrderByDescending(c => c.PopularityScore)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<Content>> SearchContentsAsync(
        string? query, ContentType? type, int page, int pageSize, CancellationToken ct = default)
    {
        var searchTerm = query?.Trim();
        var queryable = _contentRepository.QueryNoTracking();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            queryable = queryable.Where(c => c.Title.Contains(searchTerm) || (c.Description ?? string.Empty).Contains(searchTerm));
        }

        if (type.HasValue)
        {
            queryable = queryable.Where(c => c.Type == type.Value);
        }

        return await queryable
            .OrderByDescending(c => c.PopularityScore)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<List<Content>> GetTrendingAsync(int count, CancellationToken ct = default)
        => _contentRepository.QueryNoTracking().OrderByDescending(c => c.PopularityScore).Take(count).ToListAsync(ct);

    public Task<List<Rating>> GetRatingsAsync(int contentId, CancellationToken ct = default)
        => _ratingRepository.QueryNoTracking().Where(r => r.ContentId == contentId).ToListAsync(ct);

    public Task RateContentAsync(string userId, int contentId, int stars, string? review, CancellationToken ct = default)
    {
        var existing = _ratingRepository.QueryNoTracking()
            .FirstOrDefault(r => r.UserId == userId && r.ContentId == contentId);

        if (existing is null)
        {
            var rating = new Rating
            {
                UserId = userId,
                ContentId = contentId,
                Stars = stars,
                Review = review,
                CreatedAt = DateTime.UtcNow
            };
            _ratingRepository.Add(rating);
        }
        else
        {
            existing.Stars = stars;
            existing.Review = review;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task IncrementViewCountAsync(int contentId, CancellationToken ct = default)
    {
        var content = _contentRepository.QueryNoTracking().FirstOrDefault(c => c.ContentId == contentId);
        if (content is not null)
        {
            content.ViewCount++;
        }

        return Task.CompletedTask;
    }
}