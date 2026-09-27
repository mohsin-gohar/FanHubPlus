using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FanHubPlus.Data;

/// <summary>
/// Design-time factory used by the EF Core CLI:
///     dotnet ef migrations add &lt;Name&gt;
///     dotnet ef database update
///     dotnet ef migrations script
///
/// It builds the DbContext from plain configuration ONLY, so EF commands never
/// start the web app (no automatic migration, no seeding, no side effects).
/// Reads the same sources as the app: appsettings.json -> appsettings.{Env}.json
/// -> environment variables (ConnectionStrings__DefaultConnection, Database__*).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No SQL Server connection string found. Set ConnectionStrings__DefaultConnection, e.g. " +
                "\"Server=(localdb)\\MSSQLLocalDB;Database=FanHubPlus;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true\"");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString, sqlServer =>
                sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name))
            .Options;

        return new ApplicationDbContext(options);
    }
}