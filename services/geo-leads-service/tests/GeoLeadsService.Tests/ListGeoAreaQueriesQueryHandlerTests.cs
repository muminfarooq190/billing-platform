using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class ListGeoAreaQueriesQueryHandlerTests
{
    [Fact]
    public async Task ListGeoAreaQueriesQueryHandler_ShouldReturnTenantScopedRecentQuerySummaries()
    {
        var tenantId = Guid.NewGuid();
        var expected = new List<GeoAreaQueryListItem>
        {
            new(Guid.NewGuid(), "Completed", "relevance", 10, ["hotel"], 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 5, 72.82m, 18.92m, 72.84m, 18.94m),
            new(Guid.NewGuid(), "Completed", "contactability", 25, ["tour_operator"], 4, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 5, 72.80m, 18.90m, 72.88m, 18.98m)
        };

        var handler = new ListGeoAreaQueriesQueryHandler(new StubGeoAreaQueryRepository(expected), new AllowFeatureGate());

        var result = await handler.Handle(new ListGeoAreaQueriesQuery(tenantId, 10), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].RequestedLeadTypes.Should().Contain("hotel");
        result[0].PointCount.Should().Be(5);
    }

    private sealed class AllowFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class StubGeoAreaQueryRepository(IReadOnlyList<GeoAreaQueryListItem> summaries) : IGeoAreaQueryRepository
    {
        public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<GeoAreaQuery?>(null);
        public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQuery>>([]);
        public Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQueryListItem>>(summaries.Take(limit).ToList());
    }
}
