using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface ICareerService
{
    Task<JobOpening?> GetByIdAsync(int id);
    Task<List<JobOpening>> GetAllAsync();
    Task<List<JobOpening>> GetOtherJobsAsync(int excludeId, int count = 4);
}