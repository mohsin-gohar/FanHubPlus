using System.Threading.RateLimiting;
using FanHubPlus.Data;
using FanHubPlus.Models.Entities;
using FanHubPlus.Repositories;
using FanHubPlus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC services (controllers + Razor views) ----------
builder.Services.AddControllersWithViews(options =>
{
    // Global CSRF protection: every unsafe request (POST/PUT/PATCH/DELETE) must carry
    // a valid anti-forgery token. AJAX calls send it in FormData (see wwwroot/js/site.js),
    // classic forms use <form method="post"> + @Html.AntiForgeryToken().
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ---------- Response compression (HTML/JSON/CSS/JS) - smaller payloads ----------
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

// ============================================================================
//  EF Core + Microsoft SQL Server
//  Connection string comes from Configuration/environment:
//     ConnectionStrings__DefaultConnection
// ============================================================================
var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
if (!dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        $"Database:Provider='{dbProvider}' is not supported. FanHubPlus targets SQL Server; use \"SqlServer\".");
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No SQL Server connection string configured. Set 'ConnectionStrings__DefaultConnection'. " +
        "Local example: \"Server=(localdb)\\MSSQLLocalDB;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true\"");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlServer =>
    {
        sqlServer.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
        sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
        sqlServer.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    if (builder.Environment.IsDevelopment())
        options.EnableDetailedErrors();
});

// ---------- ASP.NET Core Identity ----------
// Handles: password hashing, login/logout, roles, email tokens, reset-password tokens
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password rules => strong passwords for every user (hashed, never stored as plain text)
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true; // must contain a symbol like @ # $ ...

    options.User.RequireUniqueEmail = true; // one account per email

    // Brute-force protection: lock the account for 5 minutes after 5 wrong passwords
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Production fails closed; Development keeps its explicit appsettings override.
    options.SignIn.RequireConfirmedEmail = builder.Configuration.GetValue(
        "Identity:RequireConfirmedEmail", !builder.Environment.IsDevelopment());
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders(); // creates expiring, single-use tokens for email confirm + reset password

// ---------- Auth cookie hardening ----------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.Cookie.Name = "FanHubPlus.Auth";
    options.Cookie.HttpOnly = true;                                  // JS can never read the auth cookie
    options.Cookie.SameSite = SameSiteMode.Lax;                      // blocks cross-site POST CSRF
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always; // production cookies are never sent over plain HTTP
    options.ExpireTimeSpan = TimeSpan.FromDays(7);                   // shorter than the 14-day default
    options.SlidingExpiration = true;                                // renew on activity
});

// ---------- Anti-abuse: per-IP rate limiting on auth + write + chat endpoints ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static string IpOf(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    options.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(IpOf(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 15, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));

    options.AddPolicy("write", ctx => RateLimitPartition.GetFixedWindowLimiter(IpOf(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 40, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));

    options.AddPolicy("chat", ctx => RateLimitPartition.GetFixedWindowLimiter(IpOf(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// ---------- Generic Repository (Unit of Work = shared scoped DbContext) ----------
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// ---------- Application services (Controller -> Service -> Repository -> EF Core) ----------
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IChatbotService, ChatbotService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IBookmarkService, BookmarkService>();
builder.Services.AddScoped<ISupportService, SupportService>();

// Runs automatically on startup: roles, admin user, demo user, 8 categories, demo content
builder.Services.AddScoped<DbSeeder>();

var app = builder.Build();

// ============================================================================
//  Startup: apply migrations (optional) + seed. Fails with an ACTIONABLE message
//  instead of a raw stack trace, and never drops/creates a database silently.
// ============================================================================
var startupMode = builder.Configuration["Database:StartupMode"]
    ?? (builder.Environment.IsDevelopment() ? "ApplyMigrations" : "None");

using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (startupMode.Equals("ApplyMigrations", StringComparison.OrdinalIgnoreCase))
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            startupLogger.LogInformation("Checking SQL Server schema at {Server}/{Database} ...",
                db.Database.GetDbConnection().DataSource, db.Database.GetDbConnection().Database);

            // Migrate() only ADDS pending migrations. It never drops data.
            await db.Database.MigrateAsync();
        }
        else
        {
            startupLogger.LogInformation("Database:StartupMode='{Mode}' -> schema is managed manually.", startupMode);
        }

        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex,
            "DATABASE STARTUP FAILED. Check that SQL Server is running and that " +
            "ConnectionStrings__DefaultConnection (env var) or appsettings.Development.json points to it. " +
            "Nothing was deleted - migrations only add objects. Details: {Message}", ex.Message);
        throw;
    }
}

// ---------- Middleware pipeline (order matters!) ----------

// Security headers for every response (API/JSON excluded from framing worries by SAMEORIGIN)
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed errors while coding
}
else
{
    app.UseExceptionHandler("/Home/Error"); // safe page - no stack traces in production
    app.UseHsts();                          // browser must use HTTPS in production
}

// Friendly error page for 404 / 500 (re-executes Home/Error?statusCode=...)
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();

// Static files: long cache for versioned libraries, short for uploads
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? string.Empty;
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Context.Response.Headers["Cache-Control"] = path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
            ? "public,max-age=86400"
            : "public,max-age=604800";
    }
});

app.UseRouting();

app.UseRateLimiter(); // must run AFTER routing so endpoint policies apply

app.UseAuthentication(); // 1) WHO is logged in? (reads the Identity cookie)
app.UseAuthorization();  // 2) Is he allowed?  ([Authorize], [Authorize(Roles="Admin")])

// Admin Area route first, then the normal (default) route
app.MapControllerRoute(
    name: "adminArea",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
