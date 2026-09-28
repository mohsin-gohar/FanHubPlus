using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface IBlogService
{
    Task<BlogPost?> GetByIdAsync(int id);
    Task<BlogDetailDto?> GetDetailAsync(int id);
    Task<PagedResult<BlogPost>> GetPagedAsync(BlogFilter filter);
    Task<List<BlogPost>> GetLatestAsync(int count = 4);
    Task<List<BlogPost>> GetPopularAsync(int count = 4);
    Task<List<string>> GetCategoriesAsync();
}