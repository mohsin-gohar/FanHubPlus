using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FanHubPlus.Models;

namespace FanHubPlus.Data
{
    public class FanHubDbContext : IdentityDbContext<ApplicationUser>
    {
        public FanHubDbContext(DbContextOptions<FanHubDbContext> options) : base(options) { }

        public DbSet<VideoItem> Videos => Set<VideoItem>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Channel> Channels => Set<Channel>();
        public DbSet<StoreProduct> Products => Set<StoreProduct>();
        public DbSet<JobOpening> Jobs => Set<JobOpening>();
        public DbSet<FaqItem> Faqs => Set<FaqItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<VideoItem>(entity =>
            {
                entity.HasIndex(v => v.Genre);
                entity.HasIndex(v => v.IsFeatured);
                entity.HasIndex(v => v.IsTrending);
                entity.HasIndex(v => v.IsTopRated);
                entity.HasIndex(v => v.IsLive);
                entity.HasIndex(v => v.ReleaseDate);
            });

            builder.Entity<BlogPost>(entity =>
            {
                entity.HasIndex(b => b.PublishedOn);
                entity.HasIndex(b => b.Category);
            });

            builder.Entity<StoreProduct>(entity =>
            {
                entity.HasIndex(p => p.Category);
                entity.HasIndex(p => p.Price);
            });

            builder.Entity<Channel>(entity =>
            {
                entity.HasIndex(c => c.Category);
                entity.HasIndex(c => c.Viewers);
            });

            builder.Entity<Comment>(entity =>
            {
                entity.HasIndex(c => c.VideoId);
                entity.HasIndex(c => c.ParentId);
            });

            builder.Entity<Episode>(entity =>
            {
                entity.HasIndex(e => e.VideoId);
            });

            builder.Entity<JobOpening>(entity =>
            {
                entity.HasIndex(j => j.PostedOn);
            });

            builder.Entity<FaqItem>(entity =>
            {
                entity.HasIndex(f => f.Topic);
            });
        }
    }
}