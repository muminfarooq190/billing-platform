using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.BookingChangeRequests;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingChangeRequestTests
{
    [Fact]
    public async Task CreateChangeRequest_ShouldPersistPendingRequest()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0010", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);
        var repository = new InMemoryBookingChangeRequestRepository();
        var handler = new CreateBookingChangeRequestCommandHandler(new SingleBookingRepository(booking), repository, new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        var id = await handler.Handle(new CreateBookingChangeRequestCommand(booking.TenantId, booking.Id, "DateChange", "Customer wants to shift by two days"), CancellationToken.None);

        repository.Items.Should().ContainSingle(x => x.Id == id && x.Status == "Pending");
    }

    [Fact]
    public async Task ApproveChangeRequest_ShouldMarkApproved()
    {
        var booking = Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0010", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);
        var changeRequest = BookingChangeRequest.Create(booking.Id, booking.TenantId, "DateChange", "Shift by two days");
        var repository = new InMemoryBookingChangeRequestRepository(changeRequest);
        var handler = new ApproveBookingChangeRequestCommandHandler(new SingleBookingRepository(booking), repository, new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new ApproveBookingChangeRequestCommand(booking.TenantId, booking.Id, changeRequest.Id, "Approved by ops"), CancellationToken.None);

        changeRequest.Status.Should().Be("Approved");
    }

    private sealed class InMemoryBookingChangeRequestRepository(params BookingChangeRequest[] items) : IBookingChangeRequestRepository
    {
        public List<BookingChangeRequest> Items { get; } = items.ToList();
        public Task AddAsync(BookingChangeRequest request, CancellationToken cancellationToken) { Items.Add(request); return Task.CompletedTask; }
        public Task<BookingChangeRequest?> GetByIdAsync(Guid bookingId, Guid changeRequestId, CancellationToken cancellationToken) => Task.FromResult(Items.SingleOrDefault(x => x.BookingId == bookingId && x.Id == changeRequestId));
        public Task<IReadOnlyList<BookingChangeRequest>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingChangeRequest>>(Items.Where(x => x.BookingId == bookingId).ToList());
        public Task UpdateAsync(BookingChangeRequest request, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SingleBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActorContext(Guid tenantId) : IActorContext
    {
        public Guid? UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public string? IpAddress { get; } = "127.0.0.1";
        public string? UserAgent { get; } = "tests";
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
