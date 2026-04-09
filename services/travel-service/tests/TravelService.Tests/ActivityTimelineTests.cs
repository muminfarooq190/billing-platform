using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.CreateBookingFromQuotation;
using TravelService.Application.Commands.CreateQuotationRevision;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class ActivityTimelineTests
{
    [Fact]
    public async Task CreateQuotationRevision_ShouldWriteTimelineEntry()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var activityWriter = new RecordingActivityWriter();
        var handler = new CreateQuotationRevisionCommandHandler(new InMemoryQuotationRepository(quotation), new InMemoryQuotationRevisionRepository(), activityWriter, new NoOpAuditWriter(), new FakeActorContext(quotation.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new CreateQuotationRevisionCommand(quotation.TenantId, quotation.Id, quotation.Title, quotation.Destination, quotation.TravelDate, quotation.ReturnDate, quotation.Travellers, quotation.Currency, "Visible", "Internal", DateTimeOffset.UtcNow.AddDays(10), [new QuotationRevisionLineItemDto("Flight", 1200m, 1, "USD")]), CancellationToken.None);

        activityWriter.Entries.Should().ContainSingle(x => x.EntityType == "Quotation" && x.ActivityType == "RevisionCreated");
    }

    [Fact]
    public async Task CreateBookingFromQuotation_ShouldWriteBookingAndQuotationTimelineEntries()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.Send();
        quotation.Accept(revision.Id);
        var activityWriter = new RecordingActivityWriter();
        var handler = new CreateBookingFromQuotationCommandHandler(new InMemoryQuotationRepository(quotation), new InMemoryQuotationRevisionRepository(revision), new InMemoryBookingRepository(), new InMemoryBookingStatusHistoryRepository(), activityWriter, new NoOpUnitOfWork());

        await handler.Handle(new CreateBookingFromQuotationCommand(quotation.TenantId, quotation.Id, null, "Priority booking"), CancellationToken.None);

        activityWriter.Entries.Should().Contain(x => x.EntityType == "Booking" && x.ActivityType == "BookingCreated");
        activityWriter.Entries.Should().Contain(x => x.EntityType == "Quotation" && x.ActivityType == "BookingCreated");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private sealed class RecordingActivityWriter : IActivityWriter
    {
        public List<ActivityEntry> Entries { get; } = [];
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) { Entries.Add(entry); return Task.CompletedTask; }
    }

    private sealed class InMemoryQuotationRepository(Quotation quotation) : IQuotationRepository
    {
        public Task AddAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Quotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == quotation.Id ? quotation : null);
        public Task<IReadOnlyList<Quotation>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Quotation>>([quotation]);
        public Task UpdateAsync(Quotation quotation, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryQuotationRevisionRepository(params QuotationRevision[] revisions) : IQuotationRevisionRepository
    {
        private readonly List<QuotationRevision> _revisions = revisions.ToList();
        public Task AddAsync(QuotationRevision revision, CancellationToken cancellationToken) { _revisions.Add(revision); return Task.CompletedTask; }
        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken) => Task.FromResult(_revisions.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == revisionId));
        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationRevision>>(_revisions.Where(x => x.QuotationId == quotationId).ToList());
    }

    private sealed class InMemoryBookingRepository : IBookingRepository
    {
        public List<Booking> Bookings { get; } = [];
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) { Bookings.Add(booking); return Task.CompletedTask; }
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Bookings.SingleOrDefault(x => x.Id == id));
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult(Bookings.SingleOrDefault(x => x.AcceptedRevisionId == acceptedRevisionId));
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>(Bookings.Where(x => x.TenantId == tenantId).ToList());
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryBookingStatusHistoryRepository : IBookingStatusHistoryRepository
    {
        public Task AddAsync(BookingStatusHistory history, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<BookingStatusHistory>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingStatusHistory>>([]);
    }

    private sealed class NoOpAuditWriter : IAuditWriter
    {
        public Task WriteAsync(AuditLog entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
