using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IFollowUpRepository
{
    Task AddAsync(FollowUp followUp, CancellationToken cancellationToken);
    Task<FollowUp?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FollowUp>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FollowUp>> ListOverdueAsync(DateTimeOffset asOf, CancellationToken cancellationToken);
    Task UpdateAsync(FollowUp followUp, CancellationToken cancellationToken);
}
