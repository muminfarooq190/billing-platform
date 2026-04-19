using FluentAssertions;
using GeoLeadsService.Application.Abstractions;
using GeoLeadsService.Application.Commands.SubmitGeoAreaQuery;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class SubmitGeoAreaQueryCommandHandlerTests
{
    [Fact]
    public async Task SubmitGeoAreaQueryCommandHandler_ShouldPersistCompletedQuery()
    {
        var repository = new StubGeoAreaQueryRepository();
        var handler = new SubmitGeoAreaQueryCommandHandler(repository, new StubGeoLeadCatalog(), new AllowFeatureGate());
        var polygon = new GeoPolygon(
        [
            new GeoCoordinate(72.82m, 18.92m),
            new GeoCoordinate(72.84m, 18.92m),
            new GeoCoordinate(72.84m, 18.94m),
            new GeoCoordinate(72.82m, 18.94m),
            new GeoCoordinate(72.82m, 18.92m)
        ]);

        var result = await handler.Handle(new SubmitGeoAreaQueryCommand(Guid.NewGuid(), polygon, ["hotel"], 10, "relevance"), CancellationToken.None);

        result.Count.Should().Be(1);
        repository.Stored.Should().NotBeNull();
        repository.Stored!.Results.Should().ContainSingle();
    }

    private sealed class StubGeoLeadCatalog : IGeoLeadCatalog
    {
        public Task<IReadOnlyList<GeoLead>> SearchAsync(GeoPolygon geometry, IReadOnlyList<string> leadTypes, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoLead>>(
            [
                new GeoLead(Guid.NewGuid(), "Saved Area Lead", "hotel", "hello@example.com", null, "https://example.com", "Somewhere", 18.93m, 72.83m, "Mumbai", "Maharashtra", "India", 0.9m, 0.7m, 0.8m, ["stub"], ["reason"], DateTimeOffset.UtcNow)
            ]);
    }

    private sealed class AllowFeatureGate : IFeatureGate
    {
        public Task EnsureEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> IsEnabledAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int?> GetLimitAsync(string featureKey, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<int?>(null);
    }

    private sealed class StubGeoAreaQueryRepository : IGeoAreaQueryRepository
    {
        public GeoAreaQuery? Stored { get; private set; }

        public Task AddAsync(GeoAreaQuery query, CancellationToken cancellationToken)
        {
            Stored = query;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GeoAreaQuery query, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<GeoAreaQuery?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<GeoAreaQuery?>(null);
        public Task<IReadOnlyList<GeoAreaQuery>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<GeoAreaQuery>>([]);
    }
}
