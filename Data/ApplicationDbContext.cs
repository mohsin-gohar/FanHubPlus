using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Data;

/// <summary>
/// The single class that connects ALL our tables to SQL Server.
/// It inherits IdentityDbContext so we get BOTH:
///   - Identity tables (AspNetUsers, AspNetRoles, PasswordResets...) AND
///   - our own tables (Categories, Content, Events...)
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Each DbSet = one table in the database
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UserCategory> UserCategories => Set<UserCategory>();
    public DbSet<Content> Contents => Set<Content>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContentTag> ContentTags => Set<ContentTag>();
    public DbSet<CharacterProfile> CharacterProfiles => Set<CharacterProfile>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<EventItem> Events => Set<EventItem>();
    public DbSet<MerchandiseItem> MerchandiseItems => Set<MerchandiseItem>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ChatFaq> ChatFaqs => Set<ChatFaq>();
    public DbSet<ChatbotQuery> ChatbotQueries => Set<ChatbotQuery>();
    public DbSet<FanSubmission> FanSubmissions => Set<FanSubmission>();
    public DbSet<ViewLog> ViewLogs => Set<ViewLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // MUST be called first - it creates all the Identity tables (AspNetUsers, AspNetRoles...)
        base.OnModelCreating(builder);

        // ---- Enums stored as TEXT (Movie, Pending...) instead of numbers (0, 1...)
        //      so the exported .sql script is human-readable for the judges.
        //      HasMaxLength is REQUIRED on MySQL: without it the column becomes
        //      LONGTEXT and MySQL refuses to put it in an index (Bookmarks has a
        //      unique index over ItemType) => "BLOB/TEXT column used in key
        //      specification without a key length". varchar(30) fixes that. ----
        builder.Entity<Content>().Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Entity<MediaItem>().Property(m => m.MediaType).HasConversion<string>().HasMaxLength(30);
        builder.Entity<MerchandiseItem>().Property(m => m.Tag).HasConversion<string>().HasMaxLength(30);
        builder.Entity<Feedback>().Property(f => f.Type).HasConversion<string>().HasMaxLength(30);
        builder.Entity<Feedback>().Property(f => f.Status).HasConversion<string>().HasMaxLength(30);
        builder.Entity<FanSubmission>().Property(f => f.Status).HasConversion<string>().HasMaxLength(30);
        builder.Entity<Bookmark>().Property(b => b.ItemType).HasConversion<string>().HasMaxLength(30);

        // ---- User <-> Category : many-to-many join table with a composite key ----
        builder.Entity<UserCategory>(uc =>
        {
            uc.HasKey(x => new { x.UserId, x.CategoryId }); // key = BOTH columns together

            uc.HasOne(x => x.User)
              .WithMany(u => u.FavoriteCategories)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade); // remove favorite rows when the user is deleted

            uc.HasOne(x => x.Category)
              .WithMany(c => c.FanUsers)
              .HasForeignKey(x => x.CategoryId)
              .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Content <-> Tag : many-to-many join table ----
        builder.Entity<ContentTag>(ct =>
        {
            ct.HasKey(x => new { x.ContentId, x.TagId });
            ct.HasOne(x => x.Content).WithMany(c => c.ContentTags)
              .HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
            ct.HasOne(x => x.Tag).WithMany(t => t.ContentTags)
              .HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Content -> Category : Restrict = cannot delete a category that still has content ----
        builder.Entity<Content>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Contents)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting a Content also deletes its media items (true child rows)
        builder.Entity<MediaItem>()
            .HasOne(m => m.Content)
            .WithMany(c => c.MediaItems)
            .HasForeignKey(m => m.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Characters / Articles / Merchandise also protect their category from deletion
        builder.Entity<CharacterProfile>()
            .HasOne(ch => ch.Category).WithMany(cat => cat.Characters)
            .HasForeignKey(ch => ch.CategoryId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Article>()
            .HasOne(a => a.Category).WithMany(cat => cat.Articles)
            .HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // If the author account is deleted, the article stays with AuthorId = NULL
        builder.Entity<Article>()
            .HasOne(a => a.Author).WithMany()
            .HasForeignKey(a => a.AuthorId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<MerchandiseItem>()
            .HasOne(m => m.Category).WithMany(cat => cat.Merchandise)
            .HasForeignKey(m => m.CategoryId).OnDelete(DeleteBehavior.Restrict);

        // ---- Ratings: one user can rate one content ONLY ONCE ----
        builder.Entity<Rating>(r =>
        {
            r.HasIndex(x => new { x.UserId, x.ContentId }).IsUnique();
            r.HasOne(x => x.User).WithMany()
              .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            r.HasOne(x => x.Content).WithMany(c => c.Ratings)
              .HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Bookmarks: cannot bookmark the same item twice ----
        builder.Entity<Bookmark>(b =>
        {
            b.HasIndex(x => new { x.UserId, x.ItemType, x.ItemId }).IsUnique();
            b.HasOne(x => x.User).WithMany()
              .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Feedback / chat history / view logs: user may be deleted later,
        //      so keep the row and just clear the UserId ----
        builder.Entity<Feedback>()
            .HasOne(f => f.User).WithMany()
            .HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ChatbotQuery>()
            .HasOne(q => q.User).WithMany()
            .HasForeignKey(q => q.UserId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ViewLog>()
            .HasOne(v => v.User).WithMany()
            .HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.SetNull);

        // Fan submission belongs to a user; deleting the user removes their submissions
        builder.Entity<FanSubmission>()
            .HasOne(s => s.User).WithMany()
            .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        // Useful indexes for the Explorer filters (search by title, sort by date)
        builder.Entity<Content>().HasIndex(c => c.Title);
        builder.Entity<Content>().HasIndex(c => c.ReleaseDate);
        builder.Entity<EventItem>().HasIndex(e => e.EventDate);
        builder.Entity<MerchandiseItem>().HasIndex(m => m.IsUpcoming);

        // Category name must be unique (no double "Anime")
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // SQL Server uses database-level collation; avoid MySQL-specific annotations.
        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(u => u.Id).HasMaxLength(128);
            user.Property(u => u.UserName).HasMaxLength(128);
            user.Property(u => u.NormalizedUserName).HasMaxLength(128);
            user.Property(u => u.Email).HasMaxLength(128);
            user.Property(u => u.NormalizedEmail).HasMaxLength(128);
            user.Property(u => u.ConcurrencyStamp).HasMaxLength(128);
            user.Property(u => u.SecurityStamp).HasMaxLength(128);
            user.Property(u => u.Name).HasMaxLength(100);
            user.Property(u => u.AvatarUrl).HasMaxLength(300);
            user.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
        });

        builder.Entity<IdentityRole>(role =>
        {
            role.Property(r => r.Id).HasMaxLength(128);
            role.Property(r => r.Name).HasMaxLength(128);
            role.Property(r => r.NormalizedName).HasMaxLength(128);
            role.Property(r => r.ConcurrencyStamp).HasMaxLength(128);
        });

        builder.Entity<IdentityUserToken<string>>(token =>
        {
            token.Property(t => t.UserId).HasMaxLength(128);
            token.Property(t => t.LoginProvider).HasMaxLength(128);
            token.Property(t => t.Name).HasMaxLength(128);
            token.Property(t => t.Value).HasMaxLength(512);
        });

        builder.Entity<IdentityUserLogin<string>>(login =>
        {
            login.Property(l => l.LoginProvider).HasMaxLength(128);
            login.Property(l => l.ProviderKey).HasMaxLength(128);
            login.Property(l => l.ProviderDisplayName).HasMaxLength(128);
            login.Property(l => l.UserId).HasMaxLength(128);
        });

        builder.Entity<IdentityUserClaim<string>>(claim =>
        {
            claim.Property(c => c.UserId).HasMaxLength(128);
            claim.Property(c => c.ClaimType).HasMaxLength(256);
            claim.Property(c => c.ClaimValue).HasMaxLength(1024);
        });

        builder.Entity<IdentityRoleClaim<string>>(claim =>
        {
            claim.Property(c => c.ClaimType).HasMaxLength(256);
            claim.Property(c => c.ClaimValue).HasMaxLength(1024);
        });

        // 3) Rows that are always looked up by user id + item id
        builder.Entity<UserCategory>().Property(uc => uc.UserId).HasMaxLength(128);
        builder.Entity<Rating>().Property(r => r.UserId).HasMaxLength(128);
        builder.Entity<Bookmark>().Property(b => b.UserId).HasMaxLength(128);
        builder.Entity<Bookmark>().Property(b => b.Note).HasMaxLength(500);
        builder.Entity<Feedback>().Property(f => f.UserId).HasMaxLength(128);
        builder.Entity<ChatbotQuery>().Property(q => q.UserId).HasMaxLength(128);
        builder.Entity<ViewLog>().Property(v => v.UserId).HasMaxLength(128);
        builder.Entity<FanSubmission>().Property(s => s.UserId).HasMaxLength(128);
        builder.Entity<Article>().Property(a => a.AuthorId).HasMaxLength(128);

        // SQL Server uses database-level collation; no MySQL-specific annotations.
    }
}
