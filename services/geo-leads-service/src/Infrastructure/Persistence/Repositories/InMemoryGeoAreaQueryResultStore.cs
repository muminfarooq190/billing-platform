using System.Collections.Concurrent;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;

namespace GeoLeadsService.Infrastructure.Persistence.Repositories;

public sealed class InMemoryGeoAreaQueryResultStore : IGeoAreaQueryResultStore
{
    private static readonly ConcurrentDictionary<Guid, IReadOnlyList<GeoAreaQueryResult>> Store = new();

    public Task SaveAsync(Guid queryId, IReadOnlyList<GeoAreaQueryResult> results, CancellationToken cancellationToken)
    {
        Store[queryId] = results;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<GeoAreaQueryResult>> GetAsync(Guid queryId, CancellationToken cancellationToken)
        => Task.FromResult(Store.TryGetValue(queryId, out var results) ? results : (IReadOnlyList<GeoAreaQueryResult>)[]);
}
