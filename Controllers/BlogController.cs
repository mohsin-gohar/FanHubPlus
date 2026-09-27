using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.Services;

namespace FanHubPlus.Controllers
{
    public class BlogController : Controller
    {
        private readonly IArticleService _articleService;

        public BlogController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        public async Task<IActionResult> Index(string timeline, int? categoryId, int page = 1)
        {
            var articles = await _articleService.GetPublishedAsync(timeline, categoryId, page, 12);
            var total = await _articleService.CountPublishedAsync(timeline, categoryId);
            var model = new NewsViewModel
            {
                Articles = articles,
                Timeline = timeline,
                SelectedCategoryId = categoryId,
                Page = page,
                PageSize = 12,
                TotalCount = total
            };
            return View(model);
        }

        public async Task<IActionResult> Details(string slug)
        {
            var article = await _articleService.GetBySlugAsync(slug);
            if (article == null) return NotFound();
            var model = new ArticleDetailViewModel
            {
                Article = article,
                RelatedArticles = await _articleService.GetRelatedAsync(article, 6)
            };
            return View(model);
        }
    }
}