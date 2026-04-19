using FluentAssertions;
using GeoLeadsService.Domain.Aggregates;
using GeoLeadsService.Domain.Repositories;
using Xunit;

namespace GeoLeadsService.Tests;

public sealed class SavedGeoAreaRepositoryTests
{
    [Fact]
    public async Task StubRepository_ShouldReturnTenantScopedAreas()
    {
        var tenantId = Guid.NewGuid();
        var areas = new List<SavedGeoArea>
        {
            new(tenantId, "South Mumbai", "{}"),
            new(Guid.NewGuid(), "Other Tenant", "{}")
        };

        var repository = new StubSavedGeoAreaRepository(areas);

        var result = await repository.ListByTenantAsync(tenantId, 10, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("South Mumbai");
    }

    private sealed class StubSavedGeoAreaRepository(List<SavedGeoArea> stored) : ISavedGeoAreaRepository
    {
        public Task AddAsync(SavedGeoArea area, CancellationToken cancellationToken)
        {
            stored.Add(area);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SavedGeoArea area, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteAsync(SavedGeoArea area, CancellationToken cancellationToken)
        {
            stored.Remove(area);
            return Task.CompletedTask;
        }

        public Task<SavedGeoArea?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(stored.SingleOrDefault(x => x.Id == id && x.TenantId == tenantId));

        public Task<IReadOnlyList<SavedGeoArea>> ListByTenantAsync(Guid tenantId, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<SavedGeoArea>>(stored.Where(x => x.TenantId == tenantId).Take(limit).ToList());
    }
}
