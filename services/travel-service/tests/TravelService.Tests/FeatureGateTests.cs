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
        var client = new StubBillingEntitlementsClient([
            new FeatureEntitlementDto(FeatureKeys.TravelQuotationCreate, true, "Plan", "Pro", null, DateTimeOffset.UtcNow, null, null)
        ]);
        var gate = new CachedFeatureGate(cache, client);

        var result = await gate.IsEnabledAsync(FeatureKeys.TravelQuotationCreate, tenantId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureEnabledAsync_ShouldThrow_WhenEntitlementMissing()
    {
        var tenantId = Guid.NewGuid();
        var gate = new CachedFeatureGate(new InMemoryCacheService(), new StubBillingEntitlementsClient([]));

        var act = async () => await gate.EnsureEnabledAsync(FeatureKeys.TravelAuditRead, tenantId, CancellationToken.None);

        await act.Should().ThrowAsync<TravelService.Domain.Exceptions.DomainException>().WithMessage("*not enabled*");
    }

    [Fact]
    public async Task GetLimitAsync_ShouldUseCachedBillingResponse()
    {
        var tenantId = Guid.NewGuid();
        var cache = new InMemoryCacheService();
        var client = new StubBillingEntitlementsClient([
            new FeatureEntitlementDto(FeatureKeys.TravelBookingDocumentsUpload, true, "Plan", "Pro", 25, DateTimeOffset.UtcNow, null, null)
        ]);
        var gate = new CachedFeatureGate(cache, client);

        var first = await gate.GetLimitAsync(FeatureKeys.TravelBookingDocumentsUpload, tenantId, CancellationToken.None);
        var second = await gate.GetLimitAsync(FeatureKeys.TravelBookingDocumentsUpload, tenantId, CancellationToken.None);

        first.Should().Be(25);
        second.Should().Be(25);
        client.CallCount.Should().Be(1);
    }

    private sealed class StubBillingEntitlementsClient(IReadOnlyList<FeatureEntitlementDto> items) : IBillingEntitlementsClient
    {
        public int CallCount { get; private set; }
        public Task<IReadOnlyList<FeatureEntitlementDto>> GetEffectiveEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(items);
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
