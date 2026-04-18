using BillingService.Application.Abstractions;
using BillingService.Application.Commands.GenerateInvoice;
using BillingService.Application.Commands.ProcessPayment;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using BillingService.Domain.ValueObjects;
using FluentAssertions;

namespace BillingService.Tests.Application;

public sealed class BillingMvpPassTests
{
    [Fact]
    public async Task GenerateInvoice_ShouldReuseExistingInvoice_ForSameBillingPeriod()
    {
        var subscription = Subscription.Create(Guid.NewGuid(), PlanType.Pro, BillingCycle.Monthly);
        var existing = Invoice.Generate(
            subscription.Id,
            subscription.TenantId,
            [new InvoiceLineItem("Pro plan (Monthly)", 1, new Money(49m, "USD"))],
            new Money(4.9m, "USD"),
            DateTimeOffset.UtcNow.AddDays(14),
            DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date),
            DateOnly.FromDateTime(subscription.NextBillingDate.UtcDateTime.Date.AddMonths(1).AddDays(-1)),
            "plan:Pro");

        var handler = new GenerateInvoiceCommandHandler(
            new StubSubscriptionRepository(subscription),
            new StubInvoiceRepository(existing),
            new StubPricingResolver(new BillingPricingResult(existing.LineItems, existing.TaxAmount, existing.BillingPeriodStart, existing.BillingPeriodEnd, existing.PricingReference)),
            new NoOpUnitOfWork(),
            new NoOpCacheService());

        var id = await handler.Handle(new GenerateInvoiceCommand(subscription.Id), CancellationToken.None);
        id.Should().Be(existing.Id);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnActionRequired_WhenGatewayRequiresCheckout()
    {
        var invoice = Invoice.Generate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new InvoiceLineItem("Pro plan (Monthly)", 1, new Money(49m, "USD"))],
            new Money(4.9m, "USD"),
            DateTimeOffset.UtcNow.AddDays(14),
            new DateOnly(2026, 4, 1),
            new DateOnly(2026, 4, 30),
            "plan:Pro");

        var repository = new StubInvoiceRepository(invoice);
        var handler = new ProcessPaymentCommandHandler(
            repository,
            new StubPaymentGateway(PaymentGatewayResult.RequiresAction("Stripe", "pi_123", "https://pay.local/checkout/123")),
            new NoOpUnitOfWork(),
            new NoOpCacheService());

        var result = await handler.Handle(new ProcessPaymentCommand(invoice.Id), CancellationToken.None);
        result.Should().StartWith("ActionRequired:");
        repository.Stored.Single().ProviderPaymentId.Should().Be("pi_123");
    }

    [Fact]
    public async Task BillingPricingResolver_ShouldUseFallbackPlanPricing_WhenNoPackageAssignmentExists()
    {
        var subscription = Subscription.Create(Guid.NewGuid(), PlanType.Enterprise, BillingCycle.Annual);
        var resolver = new BillingPricingResolver(new StubTenantPackageRepository(), new StubCommercialPackageRepository());

        var pricing = await resolver.ResolveAsync(subscription, CancellationToken.None);

        pricing.LineItems.Should().ContainSingle();
        pricing.LineItems[0].UnitPrice.Amount.Should().Be(1990m);
        pricing.PricingReference.Should().Be("plan:Enterprise");
    }

    private sealed class StubSubscriptionRepository(Subscription subscription) : ISubscriptionRepository
    {
        public Task AddAsync(Subscription subscription, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<Subscription?>(subscription.Id == id ? subscription : null);
        public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<Subscription?>(subscription.TenantId == tenantId ? subscription : null);
        public Task<IReadOnlyList<Subscription>> ListDueSubscriptionsAsync(DateOnly billingDate, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<Subscription>)[]);
        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubInvoiceRepository(params Invoice[] invoices) : IInvoiceRepository
    {
        public List<Invoice> Stored { get; } = invoices.ToList();
        public Task AddAsync(Invoice invoice, CancellationToken cancellationToken) { Stored.Add(invoice); return Task.CompletedTask; }
        public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Stored.SingleOrDefault(x => x.Id == id));
        public Task<Invoice?> GetBySubscriptionAndBillingPeriodAsync(Guid subscriptionId, DateOnly billingPeriodStart, DateOnly billingPeriodEnd, CancellationToken cancellationToken)
            => Task.FromResult(Stored.SingleOrDefault(x => x.SubscriptionId == subscriptionId && x.BillingPeriodStart == billingPeriodStart && x.BillingPeriodEnd == billingPeriodEnd));
        public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<Invoice>> ListOverdueCandidatesAsync(DateTimeOffset utcNow, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<Invoice>)Stored.Where(x => x.Status == InvoiceStatus.Issued && x.DueDate < utcNow).ToList());
    }

    private sealed class StubPricingResolver(BillingPricingResult pricingResult) : IBillingPricingResolver
    {
        public Task<BillingPricingResult> ResolveAsync(Subscription subscription, CancellationToken cancellationToken) => Task.FromResult(pricingResult);
    }

    private sealed class StubTenantPackageRepository : ITenantSubscriptionPackageRepository
    {
        public Task<IReadOnlyList<TenantSubscriptionPackage>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<TenantSubscriptionPackage>)[]);
        public Task<TenantSubscriptionPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<TenantSubscriptionPackage?>(null);
        public Task AddAsync(TenantSubscriptionPackage assignment, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddRangeAsync(IReadOnlyCollection<TenantSubscriptionPackage> assignments, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubCommercialPackageRepository : ICommercialPackageRepository
    {
        public Task<IReadOnlyList<CommercialPackage>> ListAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackage>)[]);
        public Task<IReadOnlyList<CommercialPackage>> ListActiveAsync(CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackage>)[]);
        public Task<CommercialPackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<CommercialPackage?>(null);
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdsAsync(IReadOnlyCollection<Guid> packageIds, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackageFeature>)[]);
        public Task<IReadOnlyList<CommercialPackageFeature>> ListFeaturesByPackageIdAsync(Guid packageId, CancellationToken cancellationToken) => Task.FromResult((IReadOnlyList<CommercialPackageFeature>)[]);
        public Task AddAsync(CommercialPackage package, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddRangeAsync(IReadOnlyCollection<CommercialPackage> packages, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task AddFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RemoveFeaturesAsync(IReadOnlyCollection<CommercialPackageFeature> features, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class StubPaymentGateway(PaymentGatewayResult result) : IPaymentGateway
    {
        public Task<PaymentGatewayResult> ProcessAsync(Guid invoiceId, Guid tenantId, Money amount, CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class NoOpUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class NoOpCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult(default(T));
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
