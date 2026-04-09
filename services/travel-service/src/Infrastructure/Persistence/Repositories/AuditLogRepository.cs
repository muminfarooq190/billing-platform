using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(TravelDbContext dbContext) : IAuditLogRepository
{
    public Task AddAsync(AuditLog entry, CancellationToken cancellationToken)
        => dbContext.AuditLogs.AddAsync(entry, cancellationToken).AsTask();
}
