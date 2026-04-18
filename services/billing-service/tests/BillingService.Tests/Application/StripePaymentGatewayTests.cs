using System.Net;
using System.Net.Http;
using System.Text;
using BillingService.Infrastructure.Payments;
using BillingService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace BillingService.Tests.Application;

public sealed class StripePaymentGatewayTests
{
    [Fact]
    public async Task StripePaymentGateway_ShouldCreateCheckoutSession_WhenConfigured()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\":\"cs_test_123\",\"url\":\"https://checkout.stripe.com/c/pay/cs_test_123\"}", Encoding.UTF8, "application/json")
        });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.stripe.com/") };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["STRIPE_SECRET_KEY"] = "sk_test_123",
            ["APP_PUBLIC_BASE_URL"] = "https://app.example.com"
        }).Build();
        var gateway = new StripePaymentGateway(client, configuration);

        var result = await gateway.ProcessAsync(Guid.NewGuid(), Guid.NewGuid(), new Money(49m, "USD"), CancellationToken.None);

        result.Status.Should().Be("RequiresAction");
        result.ProviderPaymentId.Should().Be("cs_test_123");
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/c/pay/cs_test_123");
    }

    [Fact]
    public async Task StripePaymentGateway_ShouldFail_WhenStripeReturnsError()
    {
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
        var gateway = new StripePaymentGateway(client, configuration);

        var result = await gateway.ProcessAsync(Guid.NewGuid(), Guid.NewGuid(), new Money(49m, "USD"), CancellationToken.None);

        result.Status.Should().Be("Failed");
        result.ErrorCode.Should().StartWith("stripe_http_");
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}
