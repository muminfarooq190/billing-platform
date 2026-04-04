using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class ItineraryRepository(TravelDbContext dbContext) : IItineraryRepository
{
    public Task AddAsync(Itinerary itinerary, CancellationToken cancellationToken) => dbContext.Itineraries.AddAsync(itinerary, cancellationToken).AsTask();

    public Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Itineraries.SingleOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Itinerary>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => await dbContext.Itineraries.Where(x => x.TenantId == tenantId && x.DeletedAt == null).OrderBy(x => x.StartDate).ToListAsync(cancellationToken);

    public Task UpdateAsync(Itinerary itinerary, CancellationToken cancellationToken)
    {
        dbContext.Itineraries.Update(itinerary);
        return Task.CompletedTask;
    }
}
