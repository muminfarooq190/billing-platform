using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IItineraryRepository
{
    Task AddAsync(Itinerary itinerary, CancellationToken cancellationToken);
    Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Itinerary>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task UpdateAsync(Itinerary itinerary, CancellationToken cancellationToken);
}
