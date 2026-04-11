using BillingService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<FeatureEntitlement> FeatureEntitlements => Set<FeatureEntitlement>();
    public DbSet<FeatureCatalogEntry> FeatureCatalog => Set<FeatureCatalogEntry>();
    public DbSet<CommercialPackage> CommercialPackages => Set<CommercialPackage>();
    public DbSet<CommercialPackageFeature> CommercialPackageFeatures => Set<CommercialPackageFeature>();
    public DbSet<TenantSubscriptionPackage> TenantSubscriptionPackages => Set<TenantSubscriptionPackage>();
    public DbSet<TenantFeatureOverride> TenantFeatureOverrides => Set<TenantFeatureOverride>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<OutboxMessage> DomainEvents => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
