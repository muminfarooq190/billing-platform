using TravelService.Domain.Aggregates;

namespace TravelService.Application.Abstractions;

public interface IActivityWriter
{
    Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken);
}
