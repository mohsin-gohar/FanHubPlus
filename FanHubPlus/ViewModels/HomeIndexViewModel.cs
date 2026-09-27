using FanHubPlus.Models;

namespace FanHubPlus.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<VideoItem> FeaturedHero { get; set; } = new();
        public List<VideoItem> LiveNow { get; set; } = new();
        public List<VideoItem> TrendingVideos { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Testimonial> Testimonials { get; set; } = new();
        public List<VideoItem> TopRated { get; set; } = new();
        public List<VideoItem> LatestReleases { get; set; } = new();
        public List<BlogPost> BlogPosts { get; set; } = new();
        public List<Channel> Channels { get; set; } = new();
        public List<StoreProduct> StoreDeals { get; set; } = new();
        public List<FaqItem> Faqs { get; set; } = new();
    }
}
