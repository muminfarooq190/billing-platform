using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelService.Infrastructure.Persistence;

public sealed class TravelDbContextFactory : IDesignTimeDbContextFactory<TravelDbContext>
{
    public TravelDbContext CreateDbContext(string[] args)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? TryReadDatabaseUrlFromRepositoryEnv()
            ?? "Host=localhost;Port=5432;Database=billing_travel;Username=billing_user;Password=changeme";

        var optionsBuilder = new DbContextOptionsBuilder<TravelDbContext>();
        optionsBuilder.UseNpgsql(databaseUrl);

        return new TravelDbContext(optionsBuilder.Options);
    }

    private static string? TryReadDatabaseUrlFromRepositoryEnv()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var envPath = Path.Combine(currentDirectory.FullName, ".env");
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadLines(envPath))
                {
                    if (line.StartsWith("TRAVEL_DATABASE_URL=", StringComparison.Ordinal))
                    {
                        var value = line["TRAVEL_DATABASE_URL=".Length..].Trim();
                        return value.Replace("Host=postgres", "Host=localhost", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }
}
