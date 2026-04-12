using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.AddBookingItem;
using TravelService.Application.Commands.DeleteBookingItem;
using TravelService.Application.Commands.UpdateBookingItem;
using TravelService.Application.Commands.UpdateBookingItemStatus;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class BookingItemCommandTests
{
    [Fact]
    public async Task AddBookingItem_ShouldCreateOperationalItem()
    {
        var booking = CreateBooking();
        var repository = new InMemoryBookingItemRepository();
        var handler = new AddBookingItemCommandHandler(new InMemoryBookingRepository(booking), repository, new AllowAllFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        await handler.Handle(new AddBookingItemCommand(booking.TenantId, booking.Id, "Hotel", "Rome hotel stay", "4 nights", "Example Hotels", null, "Rome", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(14), 1200m, 900m, "USD", null, null, null, "Late check-in confirmed", 1), CancellationToken.None);

        var items = await repository.ListByBookingIdAsync(booking.Id, CancellationToken.None);
        items.Should().ContainSingle();
        items[0].Status.Should().Be("Pending");
    }

    [Fact]
    public async Task UpdateBookingItemStatus_ShouldUpdateOperationalStatus()
    {
        var booking = CreateBooking();
        var item = BookingItem.Create(booking.Id, booking.TenantId, "Hotel", "Example Hotels", "Rome stay", null, "Rome", null, null, 1200m, 900m, "USD", null, 1);
        var repository = new InMemoryBookingItemRepository(item);
        var handler = new UpdateBookingItemStatusCommandHandler(new InMemoryBookingRepository(booking), repository, new AllowAllFeatureGate(), new NoOpAuditWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        await handler.Handle(new UpdateBookingItemStatusCommand(booking.TenantId, booking.Id, item.Id, "Confirmed"), CancellationToken.None);

        item.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task DeleteBookingItem_ShouldSoftDeleteItem()
    {
        var booking = CreateBooking();
        var item = BookingItem.Create(booking.Id, booking.TenantId, "Transfer", "Transfer Co", "Airport pickup", null, "Rome", null, null, null, null, null, null, 1);
        var repository = new InMemoryBookingItemRepository(item);
        var handler = new DeleteBookingItemCommandHandler(new InMemoryBookingRepository(booking), repository, new NoOpUnitOfWork());

        await handler.Handle(new DeleteBookingItemCommand(booking.TenantId, booking.Id, item.Id), CancellationToken.None);

        var items = await repository.ListByBookingIdAsync(booking.Id, CancellationToken.None);
        items.Should().BeEmpty();
    }

    private static Booking CreateBooking()
        => Booking.CreateFromAcceptedQuotation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "VOY-BKG-2026-000001", "Rome Trip", "Rome", DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(15), 2, "USD", 2500m);

    private sealed class InMemoryBookingRepository(Booking booking) : IBookingRepository
    {
        public Task AddAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == booking.Id ? booking : null);
        public Task<Booking?> GetByAcceptedRevisionIdAsync(Guid acceptedRevisionId, CancellationToken cancellationToken) => Task.FromResult<Booking?>(null);
        public Task<IReadOnlyList<Booking>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Booking>>([booking]);
        public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryBookingItemRepository(params BookingItem[] items) : IBookingItemRepository
    {
        private readonly List<BookingItem> _items = items.ToList();
        public Task AddAsync(BookingItem item, CancellationToken cancellationToken) { _items.Add(item); return Task.CompletedTask; }
        public Task<BookingItem?> GetByIdAsync(Guid bookingId, Guid itemId, CancellationToken cancellationToken) => Task.FromResult(_items.SingleOrDefault(x => x.BookingId == bookingId && x.Id == itemId && x.DeletedAt is null));
        public Task<IReadOnlyList<BookingItem>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<BookingItem>>(_items.Where(x => x.BookingId == bookingId && x.DeletedAt is null).ToList());
        public Task UpdateAsync(BookingItem item, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class AllowAllFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, Guid? userId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class NoOpActivityWriter : IActivityWriter
    {
        public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken) => Task.CompletedTask;
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


    private sealed class FakeTenantContext : TravelService.Api.ITenantContext
    {
        public Guid TenantId { get; } = Guid.NewGuid();
        public Guid? UserId { get; } = Guid.NewGuid();
    }
}
