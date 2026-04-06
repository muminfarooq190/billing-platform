using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelService.Infrastructure.Persistence;

public sealed class TravelDbContextFactory : IDesignTimeDbContextFactory<TravelDbContext>
{
    public TravelDbContext CreateDbContext(string[] args)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "Host=localhost;Port=5432;Database=billing_travel;Username=billing_user;Password=changeme";

        var optionsBuilder = new DbContextOptionsBuilder<TravelDbContext>();
        optionsBuilder.UseNpgsql(databaseUrl);

        return new TravelDbContext(optionsBuilder.Options);
    }
}
