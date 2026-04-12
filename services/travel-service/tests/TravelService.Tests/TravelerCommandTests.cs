using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.AddTraveler;
using TravelService.Application.Commands.DeleteTraveler;
using TravelService.Application.Commands.UpdateTraveler;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Tests;

public sealed class TravelerCommandTests
{
    [Fact]
    public async Task AddTraveler_ShouldSupportMultipleTravelers_ForSameBooking()
    {
        var booking = CreateBooking();
        var bookingRepository = new InMemoryBookingRepository(booking);
        var travelerRepository = new InMemoryTravelerRepository();
        var handler = new AddTravelerCommandHandler(bookingRepository, travelerRepository, new AllowAllFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        await handler.Handle(new AddTravelerCommand(booking.TenantId, booking.Id, "Jane", "Doe", null, null, "jane@example.com", null, null, null, null, null, null, null, null, true), CancellationToken.None);
        await handler.Handle(new AddTravelerCommand(booking.TenantId, booking.Id, "John", "Doe", null, null, "john@example.com", null, null, null, null, null, null, null, null, false), CancellationToken.None);

        var travelers = await travelerRepository.ListByBookingIdAsync(booking.Id, CancellationToken.None);
        travelers.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateTraveler_ShouldPersistChanges()
    {
        var booking = CreateBooking();
        var traveler = Traveler.Create(booking.Id, booking.TenantId, "Jane", "Doe", null, null, null, null, null, null, null, null, null, null, null, true);
        var handler = new UpdateTravelerCommandHandler(new InMemoryBookingRepository(booking), new InMemoryTravelerRepository(traveler), new NoOpAuditWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork());

        await handler.Handle(new UpdateTravelerCommand(booking.TenantId, booking.Id, traveler.Id, "Janet", "Doe", null, null, "janet@example.com", null, null, null, null, null, null, null, null, true), CancellationToken.None);

        traveler.FirstName.Should().Be("Janet");
        traveler.Email.Should().Be("janet@example.com");
    }

    [Fact]
    public async Task DeleteTraveler_ShouldSoftDeleteTraveler()
    {
        var booking = CreateBooking();
        var traveler = Traveler.Create(booking.Id, booking.TenantId, "Jane", "Doe", null, null, null, null, null, null, null, null, null, null, null, true);
        var travelerRepository = new InMemoryTravelerRepository(traveler);
        var handler = new DeleteTravelerCommandHandler(new InMemoryBookingRepository(booking), travelerRepository, new AllowAllFeatureGate(), new NoOpActivityWriter(), new FakeActorContext(booking.TenantId), new NoOpUnitOfWork(), new FakeTenantContext());

        await handler.Handle(new DeleteTravelerCommand(booking.TenantId, booking.Id, traveler.Id), CancellationToken.None);

        var travelers = await travelerRepository.ListByBookingIdAsync(booking.Id, CancellationToken.None);
        travelers.Should().BeEmpty();
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

    private sealed class InMemoryTravelerRepository(params Traveler[] travelers) : ITravelerRepository
    {
        private readonly List<Traveler> _travelers = travelers.ToList();

        public Task AddAsync(Traveler traveler, CancellationToken cancellationToken)
        {
            _travelers.Add(traveler);
            return Task.CompletedTask;
        }

        public Task<Traveler?> GetByIdAsync(Guid bookingId, Guid travelerId, CancellationToken cancellationToken)
            => Task.FromResult(_travelers.SingleOrDefault(x => x.BookingId == bookingId && x.Id == travelerId && x.DeletedAt is null));

        public Task<IReadOnlyList<Traveler>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<Traveler>>(_travelers.Where(x => x.BookingId == bookingId && x.DeletedAt is null).ToList());

        public Task UpdateAsync(Traveler traveler, CancellationToken cancellationToken) => Task.CompletedTask;
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
