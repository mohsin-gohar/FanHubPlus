using Microsoft.AspNetCore.Mvc;
using FanHubPlus.Models;
using FanHubPlus.Models.Entities;
using FanHubPlus.Models.ViewModels;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Controllers
{
    public class BlogController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IRepository<Category> _categories;

        public BlogController(IArticleService articleService, IRepository<Category> categories)
        {
            _articleService = articleService;
            _categories = categories;
        }

        public async Task<IActionResult> Index(string timeline, int? categoryId, int page = 1)
        {
            page = Math.Max(1, page);
            var articles = await _articleService.GetPublishedAsync(timeline, categoryId, page, 12);
            var total = await _articleService.CountPublishedAsync(timeline, categoryId);
            var model = new BlogIndexViewModel
            {
                Articles = articles,
                Categories = await _categories.Query().OrderBy(c => c.Name).ToListAsync(),
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
            var model = new BlogDetailViewModel
            {
                Article = article,
                RelatedArticles = await _articleService.GetRelatedAsync(article, 6)
            };
            return View(model);
        }
    }
}