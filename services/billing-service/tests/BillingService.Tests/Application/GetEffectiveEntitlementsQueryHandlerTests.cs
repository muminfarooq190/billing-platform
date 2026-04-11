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
            new InMemoryCommercialPackageRepository(),
            new InMemoryTenantSubscriptionPackageRepository(),
            new InMemoryTenantFeatureOverrideRepository(),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.quotation.create" && x.Granted);
        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted == false);
        result.Should().Contain(x => x.FeatureKey == "branding.assets.manage" && x.LimitValue == 25);
    }

    [Fact]
    public async Task Handle_ShouldResolveFromPackageAssignments_WhenPresent()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Free, BillingCycle.Monthly);
        var package = CommercialPackage.Create("addon.audit-plus", "Audit Plus", "Addon", "Flat", "Adds audit read.");
        var assignment = TenantSubscriptionPackage.Create(tenantId, package.Id, "Subscription", "Active", DateTimeOffset.UtcNow.AddDays(-1));
        var features = new[]
        {
            CommercialPackageFeature.Create(package.Id, "travel.audit.read", true),
            CommercialPackageFeature.Create(package.Id, "communication.notification.send", true, 2500)
        };

        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(),
            new InMemoryCommercialPackageRepository([package], features),
            new InMemoryTenantSubscriptionPackageRepository(assignment),
            new InMemoryTenantFeatureOverrideRepository(),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted);
        result.Should().Contain(x => x.FeatureKey == "communication.notification.send" && x.LimitValue == 2500);
    }

    [Fact]
    public async Task Handle_ShouldApplyActiveOverride_OverResolvedBase()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Pro, BillingCycle.Monthly);
        var overrideEntry = FeatureEntitlement.Create(tenantId, "travel.audit.read", true, EntitlementSource.AdminGrant, PlanType.Pro, null, DateTimeOffset.UtcNow.AddMinutes(-5), null, new { reason = "temporary upgrade" });
        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(overrideEntry),
            new InMemoryCommercialPackageRepository(),
            new InMemoryTenantSubscriptionPackageRepository(),
            new InMemoryTenantFeatureOverrideRepository(),
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
            new InMemoryCommercialPackageRepository(),
            new InMemoryTenantSubscriptionPackageRepository(),
            new InMemoryTenantFeatureOverrideRepository(),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted == false && x.Source == EntitlementSource.Plan.ToString());
    }

    [Fact]
    public async Task Handle_ShouldApplyTenantFeatureOverride_OverPackageResolution()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Free, BillingCycle.Monthly);
        var package = CommercialPackage.Create("addon.audit-plus", "Audit Plus", "Addon", "Flat", "Adds audit read.");
        var assignment = TenantSubscriptionPackage.Create(tenantId, package.Id, "Subscription", "Active", DateTimeOffset.UtcNow.AddDays(-1));
        var features = new[]
        {
            CommercialPackageFeature.Create(package.Id, "travel.audit.read", true)
        };
        var overrideEntry = TenantFeatureOverride.Create(tenantId, "travel.audit.read", false, "Contract restriction", "Sales", effectiveFrom: DateTimeOffset.UtcNow.AddMinutes(-10));

        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(),
            new InMemoryCommercialPackageRepository([package], features),
            new InMemoryTenantSubscriptionPackageRepository(assignment),
            new InMemoryTenantFeatureOverrideRepository(overrideEntry),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "travel.audit.read" && x.Granted == false && x.Source == EntitlementSource.Override.ToString());
    }

    [Fact]
    public async Task Handle_ShouldSumLimits_WhenPackageFeaturesUseSumPolicy()
    {
        var tenantId = Guid.NewGuid();
        var subscription = Subscription.Create(tenantId, PlanType.Free, BillingCycle.Monthly);
        var packageA = CommercialPackage.Create("addon.notifications-1", "Notifications 1", "Addon", "Flat", "Adds message allowance.");
        var packageB = CommercialPackage.Create("addon.notifications-2", "Notifications 2", "Addon", "Flat", "Adds more message allowance.");
        var assignmentA = TenantSubscriptionPackage.Create(tenantId, packageA.Id, "Subscription", "Active", DateTimeOffset.UtcNow.AddDays(-1));
        var assignmentB = TenantSubscriptionPackage.Create(tenantId, packageB.Id, "Subscription", "Active", DateTimeOffset.UtcNow.AddDays(-1));
        var features = new[]
        {
            CommercialPackageFeature.Create(packageA.Id, "communication.notification.send", true, 1000, LimitMergePolicy.Sum),
            CommercialPackageFeature.Create(packageB.Id, "communication.notification.send", true, 2500, LimitMergePolicy.Sum)
        };

        var handler = new GetEffectiveEntitlementsQueryHandler(
            new InMemorySubscriptionRepository(subscription),
            new InMemoryFeatureEntitlementRepository(),
            new InMemoryCommercialPackageRepository([packageA, packageB], features),
            new InMemoryTenantSubscriptionPackageRepository(assignmentA, assignmentB),
            new InMemoryTenantFeatureOverrideRepository(),
            new PlanEntitlementResolver());

        var result = await handler.Handle(new GetEffectiveEntitlementsQuery(tenantId), CancellationToken.None);

        result.Should().Contain(x => x.FeatureKey == "communication.notification.send" && x.LimitValue == 3500);
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

    private sealed class InMemoryCommercialPackageRepository : ICommercialPackageRepository
    {
        private readonly IReadOnlyList<CommercialPackage> _packages;
        private readonly IReadOnlyList<CommercialPackageFeature> _features;

        public InMemoryCommercialPackageRepository(IReadOnlyList<CommercialPackage>? packages = null, IReadOnlyList<CommercialPackageFeature>? features = null)
        {
            _packages = packages ?? [];
            _features = features ?? [];
        }

        public Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken) => Task.FromResult(_packages);
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<CommercialPackageFeature>>(_features.Where(x => packageIds.Contains(x.CommercialPackageId)).ToList());
        public Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryTenantSubscriptionPackageRepository(params TenantSubscriptionPackage[] assignments) : ITenantSubscriptionPackageRepository
    {
        private readonly IReadOnlyList<TenantSubscriptionPackage> _assignments = assignments;
        public Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TenantSubscriptionPackage>>(_assignments.Where(x => x.TenantId == tenantId).ToList());
        public Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryTenantFeatureOverrideRepository(params TenantFeatureOverride[] overrides) : ITenantFeatureOverrideRepository
    {
        private readonly IReadOnlyList<TenantFeatureOverride> _overrides = overrides;
        public Task AddRangeAsync(IReadOnlyCollection<TenantFeatureOverride> overrides, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<TenantFeatureOverride>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<TenantFeatureOverride>>(_overrides.Where(x => x.TenantId == tenantId).ToList());
    }
}
