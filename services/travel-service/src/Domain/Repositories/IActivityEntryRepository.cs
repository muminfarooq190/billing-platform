using TravelService.Domain.Aggregates;

namespace TravelService.Domain.Repositories;

public interface IActivityEntryRepository
{
    Task AddAsync(ActivityEntry entry, CancellationToken cancellationToken);
}
