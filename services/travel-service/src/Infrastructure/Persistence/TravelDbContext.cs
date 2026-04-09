using TravelService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence;

public sealed class TravelDbContext(DbContextOptions<TravelDbContext> options) : DbContext(options)
{
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationRevision> QuotationRevisions => Set<QuotationRevision>();
    public DbSet<QuotationRevisionLineItem> QuotationRevisionLineItems => Set<QuotationRevisionLineItem>();
    public DbSet<QuotationAttachment> QuotationAttachments => Set<QuotationAttachment>();
    public DbSet<QuotationStatusHistory> QuotationStatusHistory => Set<QuotationStatusHistory>();
    public DbSet<QuotationShareLink> QuotationShareLinks => Set<QuotationShareLink>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingStatusHistory> BookingStatusHistory => Set<BookingStatusHistory>();
    public DbSet<Traveler> Travelers => Set<Traveler>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<BookingDocument> BookingDocuments => Set<BookingDocument>();
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
