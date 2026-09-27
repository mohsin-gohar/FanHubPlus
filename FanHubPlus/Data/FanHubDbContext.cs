using Microsoft.EntityFrameworkCore;
using FanHubPlus.Models;

namespace FanHubPlus.Data
{
    public class FanHubDbContext : DbContext
    {
        public FanHubDbContext(DbContextOptions<FanHubDbContext> options) : base(options) { }

        public DbSet<VideoItem> Videos => Set<VideoItem>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<AppUser> Users => Set<AppUser>();
    }

    public class AppUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

