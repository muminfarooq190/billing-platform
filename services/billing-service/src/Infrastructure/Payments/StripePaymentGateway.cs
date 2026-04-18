using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BillingService.Application.Abstractions;
using BillingService.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace BillingService.Infrastructure.Payments;

public sealed class StripePaymentGateway(HttpClient httpClient, IConfiguration configuration) : IPaymentGateway
{
    public async Task<PaymentGatewayResult> ProcessAsync(Guid invoiceId, Guid tenantId, Money amount, CancellationToken cancellationToken)
    {
        var secretKey = configuration["STRIPE_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return PaymentGatewayResult.Failed(
                "Stripe",
                "stripe_not_configured",
                "Stripe is selected as the payment gateway but STRIPE_SECRET_KEY is not configured.");
        }

        var successUrl = configuration["STRIPE_CHECKOUT_SUCCESS_URL"] ?? configuration["APP_PUBLIC_BASE_URL"]?.TrimEnd('/') + "/billing/payment-success?invoiceId={CHECKOUT_SESSION_ID}";
        var cancelUrl = configuration["STRIPE_CHECKOUT_CANCEL_URL"] ?? configuration["APP_PUBLIC_BASE_URL"]?.TrimEnd('/') + "/billing/payment-cancelled?invoiceId={CHECKOUT_SESSION_ID}";
        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
        {
            return PaymentGatewayResult.Failed(
                "Stripe",
                "stripe_urls_not_configured",
                "Stripe checkout success/cancel URLs are not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = successUrl,
            ["cancel_url"] = cancelUrl,
            ["client_reference_id"] = invoiceId.ToString("D"),
            ["metadata[invoiceId]"] = invoiceId.ToString("D"),
            ["metadata[tenantId]"] = tenantId.ToString("D"),
            ["line_items[0][price_data][currency]"] = amount.Currency.ToLowerInvariant(),
            ["line_items[0][price_data][unit_amount]"] = Convert.ToInt64(decimal.Round(amount.Amount * 100m, 0, MidpointRounding.AwayFromZero)).ToString(),
            ["line_items[0][price_data][product_data][name]"] = $"Invoice {invoiceId:D}",
            ["line_items[0][quantity]"] = "1"
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return PaymentGatewayResult.Failed(
                "Stripe",
                $"stripe_http_{(int)response.StatusCode}",
                string.IsNullOrWhiteSpace(payload) ? "Stripe checkout session creation failed." : payload);
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        var sessionId = root.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        var checkoutUrl = root.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null;

        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return PaymentGatewayResult.Failed(
                "Stripe",
                "stripe_invalid_checkout_response",
                "Stripe returned a checkout session response without id or url.");
        }

        return PaymentGatewayResult.RequiresAction("Stripe", sessionId, checkoutUrl);
    }
}
