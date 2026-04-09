using TravelService.Domain.Aggregates;

namespace TravelService.Application.Abstractions;

public interface IAuditWriter
{
    Task WriteAsync(AuditLog entry, CancellationToken cancellationToken);
}
