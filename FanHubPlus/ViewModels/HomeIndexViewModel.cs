using FanHubPlus.DTOs;
using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<VideoListDto> FeaturedHero { get; set; } = new();
        public List<VideoListDto> LiveNow { get; set; } = new();
        public List<VideoListDto> TrendingVideos { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
        public List<VideoListDto> TopRated { get; set; } = new();
        public List<VideoListDto> LatestReleases { get; set; } = new();
        public List<BlogPost> BlogPosts { get; set; } = new();
        public List<Channel> Channels { get; set; } = new();
        public List<StoreProduct> StoreDeals { get; set; } = new();
        public List<FaqItem> Faqs { get; set; } = new();
    }
}
