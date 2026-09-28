using FanHubPlus.Models;
using FanHubPlus.Services.Interfaces;

namespace FanHubPlus.Services.Interfaces;

public interface ITestimonialService
{
    Task<List<Testimonial>> GetAllAsync();
}