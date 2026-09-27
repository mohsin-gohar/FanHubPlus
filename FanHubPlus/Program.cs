using FanHubPlus.Data;
using FanHubPlus.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<CartService>();

builder.Services.AddDbContext<FanHubDbContext>(options =>
    options.UseInMemoryDatabase("FanHubPlusDb"));

var app = builder.Build();

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FanHubDbContext>();
    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Friendly, SEO-friendly aliases for the most important pages.
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

app.Run();
