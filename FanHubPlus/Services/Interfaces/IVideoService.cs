using FanHubPlus.DTOs;
using FanHubPlus.Models;

namespace FanHubPlus.Services.Interfaces;

public interface IVideoService
{
    Task<VideoItem?> GetByIdAsync(int id);
    Task<VideoDetailDto?> GetDetailAsync(int id);
    Task<PagedResult<VideoListDto>> GetPagedAsync(VideoFilter filter);
    Task<List<VideoListDto>> GetFeaturedAsync(int count = 10);
    Task<List<VideoListDto>> GetTrendingAsync(int count = 10);
    Task<List<VideoListDto>> GetTopRatedAsync(int count = 10);
    Task<List<VideoListDto>> GetLatestAsync(int count = 10);
    Task<List<VideoListDto>> GetLiveAsync(int count = 10);
    Task<List<VideoListDto>> GetByGenreAsync(string genre, int count = 10);
    Task<List<VideoListDto>> GetRelatedAsync(int videoId, int count = 6);
    Task<List<Category>> GetCategoriesAsync();
    Task<List<Channel>> GetChannelsAsync(string? category = null);
    Task<List<Channel>> GetTopChannelsAsync(int count = 6);
    Task IncrementViewCountAsync(int videoId);
}