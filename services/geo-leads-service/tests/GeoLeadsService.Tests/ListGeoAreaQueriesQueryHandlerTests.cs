using FluentAssertions;
using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class ListGeoAreaQueriesQueryHandlerTests
{
    [Fact]
    public async Task ListGeoAreaQueriesQueryHandler_ShouldReturnTenantScopedRecentQueries()
    {
        var tenantId = Guid.NewGuid();
        var expected = new List<GeoAreaQuery>
        {
            new(tenantId, "{}", ["hotel"], 10, "relevance"),
            new(tenantId, "{}", ["tour_operator"], 25, "contactability")
        };

        var handler = new ListGeoAreaQueriesQueryHandler(new StubGeoAreaQueryRepository(expected));

        var result = await handler.Handle(new ListGeoAreaQueriesQuery(tenantId, 10), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.TenantId == tenantId);
    }

    private sealed class StubGeoAreaQueryRepository(IReadOnlyList<GeoAreaQuery> queries) : IGeoAreaQueryRepository
    {
        public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<GeoAreaQuery?>(null);
        public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQuery>>(queries.Where(x => x.TenantId == tenantId).Take(limit).ToList());
    }
}
