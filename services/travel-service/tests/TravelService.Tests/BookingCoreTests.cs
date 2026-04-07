using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.CreateBookingFromQuotation;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingCoreTests
{
    [Fact]
    public async Task CreateBookingFromQuotation_ShouldCreateBooking_ForAcceptedQuote()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.Send();
        quotation.Accept(revision.Id);

        var bookingRepository = new InMemoryBookingRepository();
        var historyRepository = new InMemoryBookingStatusHistoryRepository();
        var handler = new CreateBookingFromQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(revision),
            bookingRepository,
            historyRepository,
            new NoOpActivityWriter(),
            new NoOpUnitOfWork());

        var result = await handler.Handle(new CreateBookingFromQuotationCommand(quotation.TenantId, quotation.Id, null, "Priority booking"), CancellationToken.None);

        result.BookingId.Should().NotBe(Guid.Empty);
        bookingRepository.Bookings.Should().ContainSingle();
        bookingRepository.Bookings[0].AcceptedRevisionId.Should().Be(revision.Id);
        bookingRepository.Bookings[0].Status.Should().Be(BookingStatus.Pending);
        historyRepository.Items.Should().ContainSingle(x => x.ToStatus == "Pending");
    }

    [Fact]
    public async Task CreateBookingFromQuotation_ShouldRejectNonAcceptedQuotes()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        quotation.CreateRevision("Visible", "Internal");

        var handler = new CreateBookingFromQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(),
            new InMemoryBookingRepository(),
            new InMemoryBookingStatusHistoryRepository(),
            new NoOpActivityWriter(),
            new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new CreateBookingFromQuotationCommand(quotation.TenantId, quotation.Id, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*accepted quotation revision*");
    }

    [Fact]
    public async Task CreateBookingFromQuotation_ShouldRejectAnotherTenant()
    {
        var quotation = CreateQuotation();
        quotation.AddLineItem("Flight", 1200m, 1, "USD");
        var revision = quotation.CreateRevision("Visible", "Internal");
        quotation.Send();
        quotation.Accept(revision.Id);

        var handler = new CreateBookingFromQuotationCommandHandler(
            new InMemoryQuotationRepository(quotation),
            new InMemoryQuotationRevisionRepository(revision),
            new InMemoryBookingRepository(),
            new InMemoryBookingStatusHistoryRepository(),
            new NoOpActivityWriter(),
            new NoOpUnitOfWork());

        var act = async () => await handler.Handle(new CreateBookingFromQuotationCommand(Guid.NewGuid(), quotation.Id, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*active tenant*");
    }

    private static Quotation CreateQuotation()
        => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), "Ava", "Summer Trip", "Istanbul", DateTimeOffset.UtcNow.AddDays(20), DateTimeOffset.UtcNow.AddDays(25), 2, "USD", "notes");

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
        public Task<QuotationRevision?> GetByIdAsync(Guid quotationId, Guid revisionId, CancellationToken cancellationToken)
            => Task.FromResult(_revisions.SingleOrDefault(x => x.QuotationId == quotationId && x.Id == revisionId));
        public Task<IReadOnlyList<QuotationRevision>> ListByQuotationIdAsync(Guid quotationId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<QuotationRevision>>(_revisions.Where(x => x.QuotationId == quotationId).ToList());
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
        public List<BookingStatusHistory> Items { get; } = [];
        public Task AddAsync(BookingStatusHistory history, CancellationToken cancellationToken) { Items.Add(history); return Task.CompletedTask; }
        public Task<IReadOnlyList<BookingStatusHistory>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingStatusHistory>>(Items.Where(x => x.BookingId == bookingId).ToList());
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}
