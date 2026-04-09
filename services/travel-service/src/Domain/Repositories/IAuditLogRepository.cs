using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken cancellationToken);
}
