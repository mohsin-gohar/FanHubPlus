using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace FanHubPlus.Data;

/// <summary>
/// Runs on app startup (called from Program.cs).
/// It only inserts missing reference data, so it is safe to run many times.
/// Demo users and catalogue rows require an explicit configuration opt-in.
/// </summary>
public partial class DbSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(RoleManager<IdentityRole> roleManager,
                    UserManager<ApplicationUser> userManager,
                    ApplicationDbContext db,
                    IConfiguration configuration,
                    ILogger<DbSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // ---------- 1) Roles: Admin and User ----------
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }

        // ---------- 2) Optional local/demo accounts ----------
        // Production has no default credentials. Supply both values through environment
        // variables (or user-secrets) and opt in explicitly.
        if (_configuration.GetValue("Database:SeedDemoAccounts", false))
        {
            await EnsureConfiguredUserAsync("Database:SeedAdminEmail", "Database:SeedAdminPassword", "Site Admin", "Admin");
            await EnsureConfiguredUserAsync("Database:SeedUserEmail", "Database:SeedUserPassword", "Demo Fan", "User");
        }

        // ---------- 3) The 8 fandom categories ----------
        if (!_db.Categories.Any())
        {
            _db.Categories.AddRange(
                new Category { Name = "Anime",    Description = "Japanese animation - series, movies and OVAs." },
                new Category { Name = "Gaming",   Description = "Video games, esports and game franchises." },
                new Category { Name = "Movies",   Description = "Cinematic universes, franchises and blockbusters." },
                new Category { Name = "TV Shows", Description = "Binge-worthy television series and dramas." },
                new Category { Name = "K-Pop",    Description = "Korean pop groups, albums and comebacks." },
                new Category { Name = "Comics",   Description = "Western comics, superheroes and graphic novels." },
                new Category { Name = "Manga",    Description = "Japanese manga series and light novels." },
                new Category { Name = "Cosplay",  Description = "Cosplay culture, costumes and conventions." }
            );
            await _db.SaveChangesAsync();
        }

        // ---------- 4) Demo catalogue (contents, articles, events, merch...) ----------
        // Each block inside only inserts when its table is EMPTY, so admin
        // changes made during the demo are never overwritten on restart.
        if (_configuration.GetValue("Database:SeedDemoData", false))
            await SeedDemoDataAsync();
        else
            _logger.LogInformation("Demo catalogue seeding is disabled; reference data was left unchanged.");
    }

    /// <summary>
    /// Creates a user with password + role only if the email is not registered yet.
    /// Passwords satisfy the Identity rules in Program.cs (8 chars, upper, lower, digit, symbol).
    /// </summary>
    private async Task EnsureConfiguredUserAsync(
        string emailKey, string passwordKey, string name, string role)
    {
        var email = _configuration[emailKey]?.Trim();
        var password = _configuration[passwordKey];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning(
                "Demo account seeding was requested but {EmailKey}/{PasswordKey} is missing; account skipped.",
                emailKey, passwordKey);
            return;
        }

        if (await _userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Name = name,
            EmailConfirmed = true // explicitly configured local/demo account
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Could not seed configured user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                $"Could not assign {role}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
    }
}
