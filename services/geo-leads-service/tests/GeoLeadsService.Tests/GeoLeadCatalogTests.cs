using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using GeoLeadsService.Infrastructure.Persistence.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class GeoLeadCatalogTests
{
    [Fact]
    public async Task SeededGeoLeadCatalog_ShouldReturnLeadsInsidePolygon()
    {
        var catalog = new SeededGeoLeadCatalog(new EmptyLeadSourceRecordRepository());
        var polygon = new GeoPolygon(
        [
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

        var results = await catalog.SearchAsync(polygon, [], 10, CancellationToken.None);

        results.Should().NotBeEmpty();
        results.Should().Contain(x => x.CanonicalName == "Sunrise Adventures");
    }

    private sealed class EmptyLeadSourceRecordRepository : ILeadSourceRecordRepository
    {
        public Task AddRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task UpsertRangeAsync(IReadOnlyCollection<LeadSourceRecord> records, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<LeadSourceRecord>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceRecord>>([]);

        public Task<IReadOnlyList<LeadSourceRecord>> ListRecentAsync(int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<LeadSourceRecord>>([]);
    }
}
