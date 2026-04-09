using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.BookingFulfillment;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingFulfillmentTests
{
    [Fact]
    public async Task RequestConfirmation_ShouldMoveItemToPendingSupplier()
    {
        var booking = CreateBooking();
        var item = BookingItem.Create(booking.Id, booking.TenantId, "Hotel", "Example Hotels", "Rome hotel", null, null, null, null, 1000m, 800m, "USD", null, 1);
        var handler = new RequestBookingItemConfirmationCommandHandler(new SingleBookingRepository(booking), new SingleBookingItemRepository(item), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new RequestBookingItemConfirmationCommand(booking.TenantId, booking.Id, item.Id, DateTimeOffset.UtcNow.AddDays(2), "Awaiting supplier reply"), CancellationToken.None);

        item.Status.Should().Be("PendingSupplier");
        item.ConfirmationDeadline.Should().NotBeNull();
    }

    [Fact]
    public async Task ConfirmItem_ShouldSetConfirmationNumber()
    {
        var booking = CreateBooking();
        var item = BookingItem.Create(booking.Id, booking.TenantId, "Hotel", "Example Hotels", "Rome hotel", null, null, null, null, 1000m, 800m, "USD", null, 1);
        var handler = new ConfirmBookingItemCommandHandler(new SingleBookingRepository(booking), new SingleBookingItemRepository(item), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new ConfirmBookingItemCommand(booking.TenantId, booking.Id, item.Id, "CNF-7788", DateTimeOffset.UtcNow, "Confirmed by supplier"), CancellationToken.None);

        item.Status.Should().Be("Confirmed");
        item.ConfirmationNumber.Should().Be("CNF-7788");
        item.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task IssueItem_ShouldSetVoucherNumber()
    {
        var booking = CreateBooking();
        var item = BookingItem.Create(booking.Id, booking.TenantId, "Hotel", "Example Hotels", "Rome hotel", null, null, null, null, 1000m, 800m, "USD", null, 1);
        var handler = new IssueBookingItemCommandHandler(new SingleBookingRepository(booking), new SingleBookingItemRepository(item), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new IssueBookingItemCommand(booking.TenantId, booking.Id, item.Id, "VCH-1001", DateTimeOffset.UtcNow, "Voucher issued"), CancellationToken.None);

        item.Status.Should().Be("Issued");
        item.VoucherNumber.Should().Be("VCH-1001");
        item.IssuedAt.Should().NotBeNull();
    }

    private static Booking CreateBooking()
        => Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-0002", "Italy Trip", "Italy", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 5000m);

    private sealed class SingleBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class SingleBookingItemRepository(BookingItem item) : IBookingItemRepository
    {
        public Task AddAsync(BookingItem item, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<BookingItem?> GetByIdAsync(Guid bookingId, Guid itemId, CancellationToken cancellationToken) => Task.FromResult(bookingId == item.BookingId && itemId == item.Id ? item : null);
        public Task<IReadOnlyList<BookingItem>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingItem>>([item]);
        public Task UpdateAsync(BookingItem item, CancellationToken cancellationToken) => Task.CompletedTask;
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
