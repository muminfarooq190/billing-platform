using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;

namespace TravelService.Domain.Repositories;

public interface ITravelTemplateRepository
{
    Task AddAsync(TravelTemplate template, CancellationToken cancellationToken);
    Task<TravelTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TravelTemplate>> ListByTenantAsync(Guid tenantId, TravelTemplateContext? context, CancellationToken cancellationToken);
    Task UpdateAsync(TravelTemplate template, CancellationToken cancellationToken);
}
