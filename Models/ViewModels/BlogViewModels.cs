using FanHubPlus.Models.Entities;

namespace FanHubPlus.Models.ViewModels;

// Blog index: paged list of published articles with timeline/category filters.
// (Separate from NewsViewModel, which the News module owns with its own shape.)
public class BlogIndexViewModel
{
    public List<Article> Articles { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public string? Timeline { get; set; }              // "true" | "false" | null => all
    public int? SelectedCategoryId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// Blog detail: full article body + related stories from the same fandom
public class BlogDetailViewModel
{
    public Article Article { get; set; } = null!;
    public List<Article> RelatedArticles { get; set; } = new();
}
