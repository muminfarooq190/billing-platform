using BillingService.Application.Abstractions;
using BillingService.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace BillingService.Infrastructure.Payments;

public sealed class StripePaymentGateway(IConfiguration configuration) : IPaymentGateway
{
    public Task<PaymentGatewayResult> ProcessAsync(Guid invoiceId, Guid tenantId, Money amount, CancellationToken cancellationToken)
    {
        var publicBaseUrl = configuration["PAYMENTS_PUBLIC_BASE_URL"] ?? configuration["APP_PUBLIC_BASE_URL"] ?? "https://example.local";
        var secretKey = configuration["STRIPE_SECRET_KEY"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return Task.FromResult(PaymentGatewayResult.Failed(
                "Stripe",
                "stripe_not_configured",
                "Stripe is selected as the payment gateway but STRIPE_SECRET_KEY is not configured."));
        }

        var providerPaymentId = $"stripe_pi_{invoiceId:N}";
        var checkoutUrl = $"{publicBaseUrl.TrimEnd('/')}/billing/payments/checkout/{invoiceId:D}?tenantId={tenantId:D}";
        return Task.FromResult(PaymentGatewayResult.RequiresAction("Stripe", providerPaymentId, checkoutUrl));
    }
}
