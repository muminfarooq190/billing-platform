using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence;

public sealed class AuditWriter(IAuditLogRepository auditLogRepository) : IAuditWriter
{
    public Task WriteAsync(AuditLog entry, CancellationToken cancellationToken)
        => auditLogRepository.AddAsync(entry, cancellationToken);
}
