using System.Net;
using System.Net.Http;
using System.Text;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using BillingService.Domain.ValueObjects;
using BillingService.Infrastructure.Payments;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BillingService.Tests.Application;

public sealed class StripePaymentGatewayTests
{
    [Fact]
    public async Task StripePaymentGateway_ShouldCreateCheckoutSession_WhenConfigured()
    {
        // Stripe customer create then checkout session, in that order.
        var responses = new Queue<HttpResponseMessage>(new[]
        {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":\"cus_test_001\"}", Encoding.UTF8, "application/json") },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":\"cs_test_123\",\"url\":\"https://checkout.stripe.com/c/pay/cs_test_123\"}", Encoding.UTF8, "application/json") },
        });
        var handler = new StubHttpMessageHandler(_ => responses.Dequeue());
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.stripe.com/") };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["STRIPE_SECRET_KEY"] = "sk_test_123",
            ["APP_PUBLIC_BASE_URL"] = "https://app.example.com"
        }).Build();
        var stripeLinkRepo = new InMemoryTenantStripeLinkRepository();
        var uow = new NoopUnitOfWork();
        var gateway = new StripePaymentGateway(client, configuration, stripeLinkRepo, uow);

        var tenantId = Guid.NewGuid();
        var result = await gateway.ProcessAsync(Guid.NewGuid(), tenantId, new Money(49m, "USD"), CancellationToken.None);

        result.Status.Should().Be("RequiresAction");
        result.ProviderPaymentId.Should().Be("cs_test_123");
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/c/pay/cs_test_123");
        // Link is now persisted; second call reuses it.
        (await stripeLinkRepo.GetByTenantIdAsync(tenantId, CancellationToken.None)).Should().NotBeNull();
    }

    [Fact]
    public async Task StripePaymentGateway_ShouldReuseStripeCustomer_AcrossCheckouts()
    {
        // Only checkout response expected — no fresh customer create call.
        var handler = new StubHttpMessageHandler(req =>
        {
            req.RequestUri!.AbsolutePath.Should().Contain("checkout/sessions");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"cs_test_999\",\"url\":\"https://checkout.stripe.com/c/pay/cs_test_999\"}", Encoding.UTF8, "application/json")
            };
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.stripe.com/") };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["STRIPE_SECRET_KEY"] = "sk_test_123",
            ["APP_PUBLIC_BASE_URL"] = "https://app.example.com"
        }).Build();
        var tenantId = Guid.NewGuid();
        var stripeLinkRepo = new InMemoryTenantStripeLinkRepository();
        await stripeLinkRepo.AddAsync(TenantStripeLink.Create(tenantId, "cus_existing_42"), CancellationToken.None);
        var gateway = new StripePaymentGateway(client, configuration, stripeLinkRepo, new NoopUnitOfWork());

        var result = await gateway.ProcessAsync(Guid.NewGuid(), tenantId, new Money(49m, "USD"), CancellationToken.None);

        result.Status.Should().Be("RequiresAction");
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/c/pay/cs_test_999");
    }

    [Fact]
    public async Task StripePaymentGateway_ShouldFail_WhenStripeReturnsError()
    {
        // First HTTP call (customer create) fails — gateway short-circuits before checkout.
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":{\"message\":\"bad request\"}}", Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.stripe.com/") };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["STRIPE_SECRET_KEY"] = "sk_test_123",
            ["APP_PUBLIC_BASE_URL"] = "https://app.example.com"
        }).Build();
        var gateway = new StripePaymentGateway(client, configuration, new InMemoryTenantStripeLinkRepository(), new NoopUnitOfWork());

        var result = await gateway.ProcessAsync(Guid.NewGuid(), Guid.NewGuid(), new Money(49m, "USD"), CancellationToken.None);

        result.Status.Should().Be("Failed");
        result.ErrorCode.Should().Contain("stripe_");
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class InMemoryTenantStripeLinkRepository : ITenantStripeLinkRepository
    {
        private readonly Dictionary<Guid, TenantStripeLink> _byTenant = new();

        public Task<TenantStripeLink?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
            => Task.FromResult(_byTenant.TryGetValue(tenantId, out var link) ? link : null);

        public Task AddAsync(TenantStripeLink link, CancellationToken cancellationToken)
        {
            _byTenant[link.TenantId] = link;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TenantStripeLink link, CancellationToken cancellationToken)
        {
            _byTenant[link.TenantId] = link;
            return Task.CompletedTask;
        }
    }

    private sealed class NoopUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
    }
}
