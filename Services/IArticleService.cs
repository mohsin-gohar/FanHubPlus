using FanHubPlus.Models.Entities;

namespace FanHubPlus.Services;

// Article queries powering the Blog module: paged listing, counts,
// slug-based detail lookup and "keep reading" related stories.
public interface IArticleService
{
    // timeline: "true" => timeline stories, "false" => news posts, null/other => everything
    Task<List<Article>> GetPublishedAsync(string? timeline, int? categoryId, int page, int pageSize);
    Task<int> CountPublishedAsync(string? timeline, int? categoryId);

    // Resolves /Blog/Details/{slug}; falls back to numeric id and legacy title-based slugs
    Task<Article?> GetBySlugAsync(string slug);

    Task<List<Article>> GetRelatedAsync(Article article, int count);
}
