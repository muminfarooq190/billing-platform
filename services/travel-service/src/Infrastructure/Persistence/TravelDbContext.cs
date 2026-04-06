using TravelService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence;

public sealed class TravelDbContext(DbContextOptions<TravelDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<Itinerary> Itineraries => Set<Itinerary>();
    public DbSet<OutboxMessage> DomainEvents => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelDbContext).Assembly);
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
