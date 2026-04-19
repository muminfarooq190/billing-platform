using System.Text.Json;
using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.RefreshGeoAreaQuery;
using GeoLeadsService.Application.Queries.ListGeoAreaQueries;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class RefreshGeoAreaQueryHandlerTests
{
    [Fact]
    public async Task RefreshGeoAreaQueryCommandHandler_ShouldRecomputeAndPersistResults()
    {
        var tenantId = Guid.NewGuid();
        var polygon = new GeoPolygon(
        [
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

        var query = new GeoAreaQuery(tenantId, JsonSerializer.Serialize(polygon), [], 10, "relevance");
        var repository = new StubGeoAreaQueryRepository(query);
        var handler = new RefreshGeoAreaQueryCommandHandler(repository, new StubGeoLeadCatalog(), new AllowFeatureGate());

        var refreshed = await handler.Handle(new RefreshGeoAreaQueryCommand(tenantId, query.Id), CancellationToken.None);

        refreshed.Should().NotBeNull();
        refreshed!.Value.Count.Should().Be(1);
        repository.Stored.Results.Should().ContainSingle();
        repository.UpdateCalled.Should().BeTrue();
    }

    private sealed class AllowFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class StubGeoLeadCatalog : IGeoLeadCatalog
    {
        public Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoLead>>(
            [
                new GeoLead(Guid.NewGuid(), "Refreshed Lead", "hotel", "hello@example.com", null, "https://example.com", "Somewhere", 18.93m, 72.83m, "Mumbai", "Maharashtra", "India", 0.9m, 0.7m, 0.8m, ["stub"], ["refreshed"], DateTimeOffset.UtcNow)
            ]);
    }

    private sealed class StubGeoAreaQueryRepository(GeoAreaQuery stored) : IGeoAreaQueryRepository
    {
        public GeoAreaQuery Stored { get; private set; } = stored;
        public bool UpdateCalled { get; private set; }

        public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
        {
            Stored = query;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken)
        {
            Stored = query;
            UpdateCalled = true;
            return Task.CompletedTask;
        }

        public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(Stored.Id == id && Stored.TenantId == tenantId ? Stored : null);

        public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQuery>>(Stored.TenantId == tenantId ? [Stored] : []);
        public Task<IReadOnlyList<GeoAreaQueryListItem>> ListSummariesByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQueryListItem>>([]);
    }
}
