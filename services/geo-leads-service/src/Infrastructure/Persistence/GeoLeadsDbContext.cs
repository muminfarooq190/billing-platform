using GeoLeadsService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GeoLeadsService.Infrastructure.Persistence;

public sealed class GeoLeadsDbContext(DbContextOptions<GeoLeadsDbContext> options) : DbContext(options)
{
    public DbSet<GeoAreaQuery> GeoAreaQueries => Set<GeoAreaQuery>();
    public DbSet<GeoAreaQueryResult> GeoAreaQueryResults => Set<GeoAreaQueryResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeoAreaQuery>(builder =>
        {
            builder.ToTable("geo_area_queries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GeometryJson).HasColumnName("geometry_json");
            builder.Property(x => x.RequestedLeadTypesJson).HasColumnName("requested_lead_types_json");
            builder.Property(x => x.RequestedLimit).HasColumnName("requested_limit");
            builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
            builder.Navigation(x => x.Results).HasField("_results");
            builder.HasMany(x => x.Results).WithOne().HasForeignKey(x => x.GeoAreaQueryId);
        });

        modelBuilder.Entity<GeoAreaQueryResult>(builder =>
        {
            builder.ToTable("geo_area_query_results");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.GeoAreaQueryId).HasColumnName("geo_area_query_id");
            builder.Property(x => x.GeoLeadId).HasColumnName("geo_lead_id");
            builder.Property(x => x.Rank).HasColumnName("rank");
            builder.Property(x => x.Score).HasColumnName("score");
            builder.Property(x => x.CanonicalName).HasColumnName("canonical_name");
            builder.Property(x => x.LeadType).HasColumnName("lead_type");
            builder.Property(x => x.PrimaryEmail).HasColumnName("primary_email");
            builder.Property(x => x.PrimaryPhone).HasColumnName("primary_phone");
            builder.Property(x => x.Website).HasColumnName("website");
            builder.Property(x => x.Address).HasColumnName("address");
            builder.Property(x => x.City).HasColumnName("city");
            builder.Property(x => x.Region).HasColumnName("region");
            builder.Property(x => x.Country).HasColumnName("country");
            builder.Property(x => x.Latitude).HasColumnName("latitude");
            builder.Property(x => x.Longitude).HasColumnName("longitude");
            builder.Property(x => x.SourcesJson).HasColumnName("sources_json");
            builder.Property(x => x.ReasoningJson).HasColumnName("reasoning_json");
        });
    }
}
