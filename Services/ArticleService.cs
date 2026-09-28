using FanHubPlus.Models.Entities;
using FanHubPlus.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Services;

/// <summary>
/// Article domain logic for the Blog module. Data access goes through the generic
/// repository like every other service (controllers never touch the DbContext).
/// </summary>
public class ArticleService : IArticleService
{
    private readonly IRepository<Article> _articles;

    public ArticleService(IRepository<Article> articles)
    {
        _articles = articles;
    }

    // There is no draft state in the schema - every Article row is considered published
    // (NewsController lists the same rows unfiltered), so only the filters are applied here.
    public async Task<List<Article>> GetPublishedAsync(string? timeline, int? categoryId, int page, int pageSize)
    {
        var query = ApplyFilters(
            _articles.Query().Include(a => a.Category).Include(a => a.Author),
            timeline,
            categoryId);

        var safePage = Math.Max(1, page);
        var safeSize = pageSize <= 0 ? 12 : pageSize;

        return await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync();
    }

    public Task<int> CountPublishedAsync(string? timeline, int? categoryId)
        => ApplyFilters(_articles.Query(), timeline, categoryId).CountAsync();

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        // 1) stored slug (written on create/edit) or a numeric id ("42") passed as slug
        var article = await _articles.Query()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug || a.ArticleId.ToString() == slug);

        if (article is not null) return article;

        // 2) legacy rows seeded before the Slug column existed -> match slugified titles.
        //    The Articles table is small (editorial content), so one full scan is acceptable.
        var legacy = await _articles.Query().AsNoTracking().ToListAsync();
        return legacy.FirstOrDefault(a =>
            string.Equals(SlugHelper.Slugify(a.Title), slug, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<Article>> GetRelatedAsync(Article article, int count)
    {
        if (count <= 0) return new List<Article>();

        return await _articles.Query()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.CategoryId == article.CategoryId && a.ArticleId != article.ArticleId)
            .OrderByDescending(a => a.PublishedAt)
            .Take(count)
            .ToListAsync();
    }

    private static IQueryable<Article> ApplyFilters(IQueryable<Article> query, string? timeline, int? categoryId)
    {
        if (bool.TryParse(timeline, out var isTimeline))
            query = query.Where(a => a.IsTimeline == isTimeline);

        if (categoryId is > 0)
            query = query.Where(a => a.CategoryId == categoryId);

        return query;
    }
}
