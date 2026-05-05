using TravelService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence;

public sealed class TravelDbContext(DbContextOptions<TravelDbContext> options) : DbContext(options)
{
    public DbSet<ActivityEntry> ActivityEntries => Set<ActivityEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<EntityNote> EntityNotes => Set<EntityNote>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<TravelInquiry> TravelInquiries => Set<TravelInquiry>();
    public DbSet<TravelInquiryStatusHistory> TravelInquiryStatusHistory => Set<TravelInquiryStatusHistory>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationRevision> QuotationRevisions => Set<QuotationRevision>();
    public DbSet<QuotationRevisionLineItem> QuotationRevisionLineItems => Set<QuotationRevisionLineItem>();
    public DbSet<QuotationAttachment> QuotationAttachments => Set<QuotationAttachment>();
    public DbSet<QuotationStatusHistory> QuotationStatusHistory => Set<QuotationStatusHistory>();
    public DbSet<QuotationShareLink> QuotationShareLinks => Set<QuotationShareLink>();
    public DbSet<QuotationApprovalRequest> QuotationApprovalRequests => Set<QuotationApprovalRequest>();
    public DbSet<BookingChangeRequest> BookingChangeRequests => Set<BookingChangeRequest>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingStatusHistory> BookingStatusHistory => Set<BookingStatusHistory>();
    public DbSet<Traveler> Travelers => Set<Traveler>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<BookingDocument> BookingDocuments => Set<BookingDocument>();
    public DbSet<BookingPayment> BookingPayments => Set<BookingPayment>();
    public DbSet<Itinerary> Itineraries => Set<Itinerary>();
    public DbSet<DraftTripConcept> DraftTripConcepts => Set<DraftTripConcept>();
    public DbSet<TravelTemplate> TravelTemplates => Set<TravelTemplate>();
    public DbSet<TenantActiveTemplate> TenantActiveTemplates => Set<TenantActiveTemplate>();
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
