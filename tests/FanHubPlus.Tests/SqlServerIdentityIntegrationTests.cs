using FanHubPlus.Data;
using FanHubPlus.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FanHubPlus.Tests.Integration;

public sealed class SqlServerIdentityIntegrationTests
{
    [SkippableFact]
    public async Task Migrations_and_Identity_work_on_a_disposable_sqlserver_database()
    {
        var connectionString = Environment.GetEnvironmentVariable("FANHUBPLUS_TEST_SQLSERVER");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Skip.If(string.IsNullOrWhiteSpace(connectionString),
                "Set FANHUBPLUS_TEST_SQLSERVER to a disposable database whose name ends in _Test.");
            return;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        // UserManager requires the same Data Protection service that the web app
        // registers. Without it, Identity cannot create its password/email tokens.
        services.AddDataProtection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var email = $"integration-{Guid.NewGuid():N}@example.test";
        Assert.True((await roles.CreateAsync(new IdentityRole("Admin"))).Succeeded
                   || await roles.FindByNameAsync("Admin") is not null);

        var user = new ApplicationUser { UserName = email, Email = email, Name = "Integration Test", EmailConfirmed = true };
        Assert.True((await users.CreateAsync(user, "Test@12345")).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, "Admin")).Succeeded);
        Assert.True(await users.CheckPasswordAsync(user, "Test@12345"));
        Assert.True(await users.IsInRoleAsync(user, "Admin"));
        await users.DeleteAsync(user);
    }
}