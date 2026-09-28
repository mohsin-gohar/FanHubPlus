using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface IStoreService
{
    Task<StoreProduct?> GetByIdAsync(int id);
    Task<StoreDetailDto?> GetDetailAsync(int id);
    Task<PagedResult<StoreProduct>> GetPagedAsync(StoreFilter filter);
    Task<List<StoreProduct>> GetFeaturedAsync(int count = 3);
    Task<List<string>> GetCategoriesAsync();
}