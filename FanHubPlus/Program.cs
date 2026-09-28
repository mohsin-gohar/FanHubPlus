using FanHubPlus.Data;
using FanHubPlus.Models;
using FanHubPlus.Services;
using FanHubPlus.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=FanHubPlus.db"; // SQLite fallback for development

if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
{
    builder.Services.AddDbContext<FanHubDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<FanHubDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.EnableRetryOnFailure()));
}

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<FanHubDbContext>()
    .AddDefaultTokenProviders();

// Configure Application Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CartService>();

// Register Services
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<ICareerService, CareerService>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
        "text/css",
        "application/javascript",
        "text/javascript",
        "application/json",
        "image/svg+xml"
    ]);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("write", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FanHubDbContext>()
    .AddMemoryHealthCheck("memory");

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FanHub Plus API", Version = "v1" });
});

// HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FanHubDbContext>();
        await context.Database.MigrateAsync();
        await SeedData.InitializeAsync(context, services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FanHub Plus API v1");
    });
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000,immutable");
    }
});

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "movie-details",
    pattern: "movie/{id:int}",
    defaults: new { controller = "Home", action = "Details" });

app.MapControllerRoute(
    name: "series-details",
    pattern: "tv-series/{id:int}",
    defaults: new { controller = "Home", action = "Details" });

app.MapControllerRoute(
    name: "product-details",
    pattern: "product/{id:int}",
    defaults: new { controller = "Shop", action = "Product" });

app.MapControllerRoute(
    name: "blog-post",
    pattern: "blog/{id:int}",
    defaults: new { controller = "Home", action = "BlogDetails" });

app.MapControllerRoute(
    name: "career",
    pattern: "career/{id:int}",
    defaults: new { controller = "Home", action = "CareerDetails" });

app.MapControllerRoute(
    name: "service",
    pattern: "service/{id:int}",
    defaults: new { controller = "Home", action = "ServiceDetails" });

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = Microsoft.AspNetCore.Diagnostics.HealthChecks.UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();