using GeoLeadsService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence;

public sealed class GeoLeadsDbContext(DbContextOptions<GeoLeadsDbContext> options) : DbContext(options)
{
    public DbSet<GeoAreaQuery> GeoAreaQueries => Set<GeoAreaQuery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeoAreaQuery>(builder =>
        {
            builder.ToTable("geo_area_queries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GeometryJson).HasColumnName("geometry_json");
            builder.Property(x => x.RequestedLimit).HasColumnName("requested_limit");
            builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
            builder.Ignore(x => x.RequestedLeadTypes);
            builder.Ignore(x => x.Results);
        });
    }
}
