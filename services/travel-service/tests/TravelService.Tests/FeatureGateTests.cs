using FluentAssertions;
using TravelService.Application.Abstractions;
using TravelService.Infrastructure.Entitlements;

namespace TravelService.Tests;

public sealed class FeatureGateTests
{
    [Fact]
    public async Task IsEnabledAsync_ShouldReturnTrue_WhenEntitlementGranted()
    {
        var tenantId = Guid.NewGuid();
        var cache = new InMemoryCacheService();
        var client = new StubBillingEntitlementsClient(
            [new FeatureEntitlementDto(FeatureKeys.TravelQuotationCreate, true, "Plan", "Pro", null, DateTimeOffset.UtcNow, null, null)],
            []);
        var gate = new CachedFeatureGate(cache, client);

        var result = await gate.IsEnabledAsync(FeatureKeys.TravelQuotationCreate, tenantId, null, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureEnabledAsync_ShouldThrow_WhenUserAccessMissing()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var gate = new CachedFeatureGate(
            new InMemoryCacheService(),
            new StubBillingEntitlementsClient(
                [],
                [new UserFeatureAccessDto(tenantId, userId, FeatureKeys.TravelAuditRead, true, false, true, false, null, "ExplicitUserAssignment")]));

        var act = async () => await gate.EnsureEnabledAsync(FeatureKeys.TravelAuditRead, tenantId, userId, CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetLimitAsync_ShouldUseCachedBillingResponse()
    {
        var tenantId = Guid.NewGuid();
        var cache = new InMemoryCacheService();
        var client = new StubBillingEntitlementsClient(
            [new FeatureEntitlementDto(FeatureKeys.TravelBookingDocumentsUpload, true, "Plan", "Pro", 25, DateTimeOffset.UtcNow, null, null)],
            []);
        var gate = new CachedFeatureGate(cache, client);

        var first = await gate.GetLimitAsync(FeatureKeys.TravelBookingDocumentsUpload, tenantId, null, CancellationToken.None);
        var second = await gate.GetLimitAsync(FeatureKeys.TravelBookingDocumentsUpload, tenantId, null, CancellationToken.None);

        first.Should().Be(25);
        second.Should().Be(25);
        client.EntitlementCallCount.Should().Be(1);
    }

    private sealed class StubBillingEntitlementsClient(IReadOnlyList<FeatureEntitlementDto> entitlements, IReadOnlyList<UserFeatureAccessDto> userAccess) : IBillingEntitlementsClient
    {
        public int EntitlementCallCount { get; private set; }
        public int UserAccessCallCount { get; private set; }

        public Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            EntitlementCallCount++;
            return Task.FromResult(entitlements);
        }

        public Task<IReadOnlyList<UserFeatureAccessDto>> GetUserFeatureAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
        {
            UserAccessCallCount++;
            return Task.FromResult(userAccess);
        }
    }

    private sealed class InMemoryCacheService : ICacheService
    {
        private readonly Dictionary<string, object?> _items = new();
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
            => Task.FromResult(_items.TryGetValue(key, out var value) ? (T?)value : default);
        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken)
        {
            _items[key] = value;
            return Task.CompletedTask;
        }
        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            _items.Remove(key);
            return Task.CompletedTask;
        }
    }
}
