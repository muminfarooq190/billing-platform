using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface ITravelerRepository
{
    Task AddAsync(Traveler traveler, CancellationToken cancellationToken);
    Task<Traveler?> GetByIdAsync(Guid bookingId, Guid travelerId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Traveler>> ListByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken);
    Task UpdateAsync(Traveler traveler, CancellationToken cancellationToken);
}
