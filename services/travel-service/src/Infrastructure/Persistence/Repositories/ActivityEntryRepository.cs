using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Infrastructure.Persistence.Repositories;

public sealed class ActivityEntryRepository(TravelDbContext dbContext) : IActivityEntryRepository
{
    public Task AddAsync(ActivityEntry entry, CancellationToken cancellationToken)
        => dbContext.ActivityEntries.AddAsync(entry, cancellationToken).AsTask();
}
