using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface IFaqService
{
    Task<List<FaqItem>> GetAllAsync(string? topic = null);
    Task<List<string>> GetTopicsAsync();
    Task<List<FaqItem>> GetPopularAsync(int count = 4);
}