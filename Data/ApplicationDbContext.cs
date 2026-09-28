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
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistItem> PlaylistItems => Set<PlaylistItem>();

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

        // ---- Playlists & PlaylistItems ----
        builder.Entity<Playlist>(p =>
        {
            p.HasOne(x => x.User).WithMany()
              .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PlaylistItem>(pi =>
        {
            pi.HasOne(x => x.Playlist).WithMany(p => p.Items)
              .HasForeignKey(x => x.PlaylistId).OnDelete(DeleteBehavior.Cascade);
            pi.HasOne(x => x.Content).WithMany()
              .HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
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
        builder.Entity<Content>().HasIndex(c => new { c.CategoryId, c.Type });
        builder.Entity<Content>().HasIndex(c => c.Type);
        builder.Entity<Content>().HasIndex(c => new { c.PopularityScore, c.ViewCount });

        builder.Entity<CharacterProfile>().HasIndex(c => c.Name);
        builder.Entity<CharacterProfile>().HasIndex(c => c.CategoryId);
        builder.Entity<CharacterProfile>().HasIndex(c => c.Fandom);

        builder.Entity<EventItem>().HasIndex(e => e.EventDate);
        builder.Entity<EventItem>().HasIndex(e => e.City);

        builder.Entity<MerchandiseItem>().HasIndex(m => m.IsUpcoming);
        builder.Entity<MerchandiseItem>().HasIndex(m => new { m.CategoryId, m.Tag });

        builder.Entity<Article>().HasIndex(a => a.Slug);   // /Blog/Details/{slug} lookups
        builder.Entity<Article>().HasIndex(a => a.PublishedAt);
        builder.Entity<Article>().HasIndex(a => new { a.IsTimeline, a.CategoryId });

        builder.Entity<FanSubmission>().HasIndex(s => s.Status);
        builder.Entity<FanSubmission>().HasIndex(s => s.CreatedAt);

        // Category name must be unique (no double "Anime")
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // ---- ASP.NET Core Identity column widths -----------------------------
        // These MUST match the widths the database was actually created with
        // (see Migrations/20260924095153_InitialCreate), otherwise SQL Server
        // rejects any new foreign key:
        //   "Column 'AspNetUsers.Id' is not the same length or scale as
        //    referencing column 'Playlists.UserId' in foreign key ..."
        // SQL Server's Identity defaults are 450 for keys and 256 for names;
        // the 128 values that used to sit here came from the MySQL era and
        // never had a migration, so the model silently drifted from the schema.
        const int KeyLength = 450;   // Id / UserId / RoleId and other GUID keys
        const int NameLength = 256;  // UserName / Email / Normalized* / Role name

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(u => u.Id).HasMaxLength(KeyLength);
            user.Property(u => u.UserName).HasMaxLength(NameLength);
            user.Property(u => u.NormalizedUserName).HasMaxLength(NameLength);
            user.Property(u => u.Email).HasMaxLength(NameLength);
            user.Property(u => u.NormalizedEmail).HasMaxLength(NameLength);
            user.Property(u => u.Name).HasMaxLength(100);
            user.Property(u => u.AvatarUrl).HasMaxLength(300);
            user.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
        });

        builder.Entity<IdentityRole>(role =>
        {
            role.Property(r => r.Id).HasMaxLength(KeyLength);
            role.Property(r => r.Name).HasMaxLength(NameLength);
            role.Property(r => r.NormalizedName).HasMaxLength(NameLength);
        });

        builder.Entity<IdentityUserToken<string>>(token =>
        {
            token.Property(t => t.UserId).HasMaxLength(KeyLength);
            token.Property(t => t.LoginProvider).HasMaxLength(KeyLength);
            token.Property(t => t.Name).HasMaxLength(KeyLength);
        });

        builder.Entity<IdentityUserLogin<string>>(login =>
        {
            login.Property(l => l.LoginProvider).HasMaxLength(KeyLength);
            login.Property(l => l.ProviderKey).HasMaxLength(KeyLength);
            login.Property(l => l.UserId).HasMaxLength(KeyLength);
        });

        builder.Entity<IdentityUserClaim<string>>(claim =>
        {
            claim.Property(c => c.UserId).HasMaxLength(KeyLength);
        });

        // 3) Rows that are always looked up by user id + item id
        builder.Entity<UserCategory>().Property(uc => uc.UserId).HasMaxLength(KeyLength);
        builder.Entity<Rating>().Property(r => r.UserId).HasMaxLength(KeyLength);
        builder.Entity<Bookmark>().Property(b => b.UserId).HasMaxLength(KeyLength);
        builder.Entity<Bookmark>().Property(b => b.Note).HasMaxLength(500);
        builder.Entity<Feedback>().Property(f => f.UserId).HasMaxLength(KeyLength);
        builder.Entity<ChatbotQuery>().Property(q => q.UserId).HasMaxLength(KeyLength);
        builder.Entity<ViewLog>().Property(v => v.UserId).HasMaxLength(KeyLength);
        builder.Entity<FanSubmission>().Property(s => s.UserId).HasMaxLength(KeyLength);
        builder.Entity<Article>().Property(a => a.AuthorId).HasMaxLength(KeyLength);
        builder.Entity<Playlist>().Property(p => p.UserId).HasMaxLength(KeyLength);
    }
}
