using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.AcceptQuotation;
using TravelService.Application.Commands.CreateQuotationRevision;
using TravelService.Application.Commands.UpdateTraveler;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class AuditLogTests
{
    [Fact]
    public async Task AcceptQuotation_ShouldWriteAuditLog()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.Send();
        var auditWriter = new RecordingAuditWriter();
        var actorContext = new FakeActorContext(quotation.TenantId);
        var handler = new AcceptQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(revision),
            new InMemoryQuotationStatusHistoryRepository(),
            auditWriter,
            actorContext,
            new NoOpUnitOfWork());

        await handler.Handle(new AcceptQuotationCommand(quotation.TenantId, quotation.Id, revision.Id, "Customer approved"), CancellationToken.None);

        auditWriter.Entries.Should().ContainSingle(x => x.EntityType == "Quotation" && x.Action == "Accepted");
        auditWriter.Entries[0].MetadataJson.Should().Contain("Customer approved");
    }

    [Fact]
    public async Task UpdateTraveler_ShouldWriteAuditLogWithBeforeAndAfter()
    {
        var booking = CreateBooking();
        var traveler = Traveler.Create(booking.Id, booking.TenantId, "Jane", "Doe", null, null, null, null, null, null, null, null, null, null, null, true);
        var auditWriter = new RecordingAuditWriter();
        var actorContext = new FakeActorContext(booking.TenantId);
        var handler = new UpdateTravelerCommandHandler(
            new InMemoryBookingRepository(booking),
            new InMemoryTravelerRepository(traveler),
            auditWriter,
            actorContext,
            new NoOpUnitOfWork());

        await handler.Handle(new UpdateTravelerCommand(booking.TenantId, booking.Id, traveler.Id, "Janet", "Doe", null, null, "janet@example.com", null, null, null, null, null, null, null, null, true), CancellationToken.None);

        auditWriter.Entries.Should().ContainSingle(x => x.EntityType == "Traveler" && x.Action == "Updated");
        auditWriter.Entries[0].BeforeJson.Should().Contain("Jane");
        auditWriter.Entries[0].AfterJson.Should().Contain("Janet");
    }

    [Fact]
    public async Task CreateQuotationRevision_ShouldWriteAuditLog()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var auditWriter = new RecordingAuditWriter();
        var actorContext = new FakeActorContext(quotation.TenantId);
        var handler = new CreateQuotationRevisionCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(),
            new AllowAllFeatureGate(),
            new NoOpActivityWriter(),
            auditWriter,
            actorContext,
            new NoOpUnitOfWork());

        await handler.Handle(new CreateQuotationRevisionCommand(quotation.TenantId, quotation.Id, quotation.Title, quotation.Destination, quotation.TravelDate, quotation.ReturnDate, quotation.Travellers, quotation.Currency, "Visible", "Internal", DateTimeOffset.UtcNow.AddDays(10), [new QuotationRevisionLineItemDto("Flight", 1200m, 1, "USD")]), CancellationToken.None);

        auditWriter.Entries.Should().ContainSingle(x => x.EntityType == "Quotation" && x.Action == "RevisionCreated");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

    private static Booking CreateBooking()
        => Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-000001", "Rome Trip", "Rome", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 2500m);

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class RecordingAuditWriter : IAuditWriter
    {
        public List<AuditLog> Entries { get; } = [];
        public Task WriteAsync(AuditLog entry, CancellationToken cancellationToken) { Entries.Add(entry); return Task.CompletedTask; }
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
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

    private sealed class InMemoryQuotationStatusHistoryRepository : IQuotationStatusHistoryRepository
    {
        public Task AddAsync(QuotationStatusHistory history, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<QuotationStatusHistory>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<QuotationStatusHistory>>([]);
    }

    private sealed class InMemoryBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryTravelerRepository(params Traveler[] travelers) : ITravelerRepository
    {
        private readonly List<Traveler> _travelers = travelers.ToList();
        public Task AddAsync(Traveler traveler, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Traveler?> GetByIdAsync(Guid bookingId, Guid travelerId, CancellationToken cancellationToken)
            => Task.FromResult(_travelers.SingleOrDefault(x => x.BookingId == bookingId && x.Id == travelerId && x.DeletedAt is null));
        public Task<IReadOnlyList<Traveler>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Traveler>>(_travelers.Where(x => x.BookingId == bookingId && x.DeletedAt is null).ToList());
        public Task UpdateAsync(Traveler traveler, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
