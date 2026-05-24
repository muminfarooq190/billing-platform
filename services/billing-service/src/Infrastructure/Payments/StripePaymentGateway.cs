using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using BillingService.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace BillingService.Infrastructure.Payments;

/// <summary>
/// Stripe-backed implementation of <see cref="IPaymentGateway"/>.
///
/// Each <c>ProcessAsync</c> call:
///   1. Looks up the tenant's saved <c>stripe_customer_id</c> in
///      <c>tenant_stripe_links</c>. If absent, creates a fresh Stripe
///      Customer (<c>POST /v1/customers</c>) and persists the link.
///   2. Creates a Checkout Session bound to that Customer so saved payment
///      methods + receipts roll up per-tenant on the Stripe dashboard.
///
/// Without the Customer link every checkout used to spawn an anonymous
/// Customer — card-on-file, future-invoice billing, and the "saved payment
/// methods" surface were all broken because each invoice looked like a
/// different shopper to Stripe.
/// </summary>
public sealed class StripePaymentGateway(
    HttpClient httpClient,
    IConfiguration configuration,
    ITenantStripeLinkRepository tenantStripeLinkRepository,
    IUnitOfWork unitOfWork) : IPaymentGateway
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

        // -- 1. Resolve (or lazily create) the Stripe Customer for this tenant --
        var customerResult = await ResolveOrCreateCustomerAsync(secretKey, tenantId, cancellationToken);
        if (!customerResult.Success)
        {
            return PaymentGatewayResult.Failed("Stripe", customerResult.ErrorCode!, customerResult.ErrorMessage!);
        }
        var stripeCustomerId = customerResult.CustomerId!;

        // -- 2. Create Checkout Session bound to the Customer ------------------
        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["customer"] = stripeCustomerId,
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

    /// <summary>
    /// Returns the cached Stripe Customer id for this tenant, or creates one
    /// via <c>POST /v1/customers</c> and persists the link. Idempotent across
    /// concurrent calls — last-writer-wins on the link row is acceptable
    /// (Stripe charges no penalty for orphan Customers; we'd just leak one).
    /// </summary>
    private async Task<CustomerLookupResult> ResolveOrCreateCustomerAsync(string secretKey, Guid tenantId, CancellationToken cancellationToken)
    {
        var existing = await tenantStripeLinkRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (existing is not null) return CustomerLookupResult.Found(existing.StripeCustomerId);

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/customers");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["metadata[tenantId]"] = tenantId.ToString("D"),
            ["description"] = $"Voyara tenant {tenantId:D}",
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return CustomerLookupResult.Error(
                $"stripe_customer_create_http_{(int)response.StatusCode}",
                string.IsNullOrWhiteSpace(payload) ? "Stripe customer creation failed." : payload);
        }

        using var document = JsonDocument.Parse(payload);
        var customerId = document.RootElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return CustomerLookupResult.Error("stripe_customer_missing_id", "Stripe returned a customer response without id.");
        }

        var link = TenantStripeLink.Create(tenantId, customerId);
        await tenantStripeLinkRepository.AddAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CustomerLookupResult.Found(customerId);
    }

    private readonly record struct CustomerLookupResult(bool Success, string? CustomerId, string? ErrorCode, string? ErrorMessage)
    {
        public static CustomerLookupResult Found(string customerId) => new(true, customerId, null, null);
        public static CustomerLookupResult Error(string code, string message) => new(false, null, code, message);
    }
}
