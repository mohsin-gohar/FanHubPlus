using FanHubPlus.Core.Entities;
using FanHubPlus.Core.Enums;

namespace FanHubPlus.Application.Interfaces;

/// <summary>
/// Domain service contract for content discovery, playback and rating.
/// </summary>
public interface IContentService
{
    Task<Content?> GetContentByIdAsync(int contentId, CancellationToken ct = default);

    Task<List<Content>> GetContentsByCategoryAsync(
        int categoryId, ContentType? type, int page, int pageSize, CancellationToken ct = default);

    Task<List<Content>> SearchContentsAsync(
        string? query, ContentType? type, int page, int pageSize, CancellationToken ct = default);

    Task<List<Content>> GetTrendingAsync(int count, CancellationToken ct = default);

    Task<List<Rating>> GetRatingsAsync(int contentId, CancellationToken ct = default);

    Task RateContentAsync(string userId, int contentId, int stars, string? review, CancellationToken ct = default);

    Task IncrementViewCountAsync(int contentId, CancellationToken ct = default);
}