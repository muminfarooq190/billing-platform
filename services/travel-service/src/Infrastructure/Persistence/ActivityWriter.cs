using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence;

public sealed class ActivityWriter(IActivityEntryRepository activityEntryRepository) : IActivityWriter
{
    public Task WriteAsync(ActivityEntry entry, CancellationToken cancellationToken)
        => activityEntryRepository.AddAsync(entry, cancellationToken);
}
