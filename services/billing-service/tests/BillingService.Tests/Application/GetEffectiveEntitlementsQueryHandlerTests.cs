using BillingService.Application.Abstractions;
using BillingService.Application.Queries.GetEffectiveEntitlements;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using BillingService.Infrastructure.Entitlements;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class GetEffectiveEntitlementsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldResolvePlanDefaults_ForProPlan()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly);
        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.quotation.create" && x.Granted);
        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted == false);
        result.Should().Contain(x => x.FeatureKey == "branding.assets.manage" && x.LimitValue == 25);
    }

    [Fact]
    public async Task Handle_ShouldApplyActiveOverride_OverPlanDefault()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly);
        var overrideEntry = FeatureEntitlement.Create(tenantId, "travel.audit.read", true, EntitlementSource.AdminGrant, PlanType.Pro, null, DateTimeOffset.UtcNow.AddMinutes(-5), null, new { reason = "temporary upgrade" });
        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(overrideEntry),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted && x.Source == EntitlementSource.AdminGrant.ToString());
    }

    [Fact]
    public async Task Handle_ShouldIgnoreExpiredOverride()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly);
        var expiredOverride = FeatureEntitlement.Create(tenantId, "travel.audit.read", true, EntitlementSource.AdminGrant, PlanType.Pro, null, DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1));
        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(expiredOverride),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted == false && x.Source == EntitlementSource.Plan.ToString());
    }

    private sealed class InMemorySubscriptionRepository(Subscription subscription) : ISubscriptionRepository
    {
        public Task AddAsync(Subscription subscription, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == subscription.Id ? subscription : null);
        public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(tenantId == subscription.TenantId ? subscription : null);
        public Task<IReadOnlyList<Subscription>> ListDueSubscriptionsAsync(DateOnly billingDate, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Subscription>>([subscription]);
        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryFeatureEntitlementRepository(params FeatureEntitlement[] entitlements) : IFeatureEntitlementRepository
    {
        private readonly IReadOnlyList<FeatureEntitlement> _entitlements = entitlements;
        public Task AddRangeAsync(IReadOnlyCollection<FeatureEntitlement> entitlements, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<FeatureEntitlement>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<FeatureEntitlement>>(_entitlements.Where(x => x.TenantId == tenantId).ToList());
    }
}
