using FanHubPlus.Core.Entities;
using FanHubPlus.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Content> Contents => Set<Content>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<UserCategory> UserCategories => Set<UserCategory>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Content>()
            .HasOne(c => c.Category)
            .WithMany(c => c.Contents)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MediaItem>()
            .HasOne(m => m.Content)
            .WithMany(c => c.MediaItems)
            .HasForeignKey(m => m.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.Content)
            .WithMany(c => c.Ratings)
            .HasForeignKey(r => r.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContentTag>()
            .HasKey(ct => new { ct.ContentId, ct.TagId });

        modelBuilder.Entity<ContentTag>()
            .HasOne(ct => ct.Content)
            .WithMany(c => c.ContentTags)
            .HasForeignKey(ct => ct.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ContentTag>()
            .HasOne(ct => ct.Tag)
            .WithMany(t => t.ContentTags)
            .HasForeignKey(ct => ct.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCategory>()
            .HasKey(uc => new { uc.UserId, uc.CategoryId });

        modelBuilder.Entity<UserCategory>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserCategories)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCategory>()
            .HasOne(uc => uc.Category)
            .WithMany(c => c.FanUsers)
            .HasForeignKey(uc => uc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}