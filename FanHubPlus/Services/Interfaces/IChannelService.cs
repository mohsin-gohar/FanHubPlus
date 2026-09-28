using FanHubPlus.DTOs;
using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface IChannelService
{
    Task<Channel?> GetByIdAsync(int id);
    Task<PagedResult<Channel>> GetPagedAsync(ChannelFilter filter);
    Task<List<Channel>> GetTopAsync(int count = 6);
    Task<List<string>> GetCategoriesAsync();
}