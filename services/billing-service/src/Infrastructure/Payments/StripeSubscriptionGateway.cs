using System.Net.Http.Headers;
using System.Text.Json;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;

namespace BillingService.Infrastructure.Payments;

/// <summary>
/// Stripe-native subscription gateway. Replaces the cron-driven invoice
/// generator for packages that have <c>StripePriceIdMonthly/Annual</c>
/// wired.
///
/// Creates a `mode=subscription` Stripe Checkout Session bound to:
///   - the tenant's persistent Stripe Customer (lazy-created via the
///     same <see cref="TenantStripeLink"/> path the one-shot gateway uses)
///   - the package's Stripe Price id for the chosen cycle
///
/// Stripe then owns the schedule:
///   - charges the saved card on each renewal
///   - emits `customer.subscription.created` / `.updated` / `.deleted`
///   - emits `invoice.payment_succeeded` / `.payment_failed` per cycle
///
/// Our cron (`BillingSchedulerService`) checks <c>IsManagedByStripe</c>
/// and skips Stripe-managed subscriptions so we don't double-bill.
/// </summary>
public sealed class StripeSubscriptionGateway(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ITenantStripeLinkRepository tenantStripeLinkRepository,
    IUnitOfWork unitOfWork,
    ILogger<StripeSubscriptionGateway> logger)
{
    public sealed record SubscriptionCheckoutResult(string SessionId, string CheckoutUrl, string StripeCustomerId);

    public async Task<SubscriptionCheckoutResult?> CreateCheckoutAsync(
        Guid tenantId,
        Guid subscriptionId,
        string stripePriceId,
        CancellationToken cancellationToken)
    {
        var secretKey = configuration["STRIPE_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            logger.LogWarning("StripeSubscriptionGateway invoked but STRIPE_SECRET_KEY is not configured.");
            return null;
        }
        if (string.IsNullOrWhiteSpace(stripePriceId))
        {
            logger.LogWarning("CreateCheckoutAsync called without a Stripe Price id — package not wired for recurring.");
            return null;
        }

        var successUrl = configuration["STRIPE_CHECKOUT_SUCCESS_URL"] ?? configuration["APP_PUBLIC_BASE_URL"]?.TrimEnd('/') + "/billing/payment-success?sessionId={CHECKOUT_SESSION_ID}";
        var cancelUrl = configuration["STRIPE_CHECKOUT_CANCEL_URL"] ?? configuration["APP_PUBLIC_BASE_URL"]?.TrimEnd('/') + "/billing/payment-cancelled?sessionId={CHECKOUT_SESSION_ID}";

        var customerId = await ResolveOrCreateCustomerAsync(secretKey, tenantId, cancellationToken);
        if (customerId is null)
        {
            logger.LogError("Could not resolve Stripe customer for tenant {TenantId}.", tenantId);
            return null;
        }

        using var client = httpClientFactory.CreateClient("stripe-refunds");
        if (client.BaseAddress is null) client.BaseAddress = new Uri(configuration["STRIPE_API_BASE_URL"] ?? "https://api.stripe.com/");

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/checkout/sessions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["mode"] = "subscription",
            ["customer"] = customerId,
            ["success_url"] = successUrl!,
            ["cancel_url"] = cancelUrl!,
            ["line_items[0][price]"] = stripePriceId,
            ["line_items[0][quantity]"] = "1",
            ["client_reference_id"] = subscriptionId.ToString("D"),
            ["metadata[subscriptionId]"] = subscriptionId.ToString("D"),
            ["metadata[tenantId]"] = tenantId.ToString("D"),
            // Pass subscription metadata so webhooks can correlate back to
            // our row without an extra lookup.
            ["subscription_data[metadata][subscriptionId]"] = subscriptionId.ToString("D"),
            ["subscription_data[metadata][tenantId]"] = tenantId.ToString("D"),
        });

        using var response = await client.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Stripe subscription checkout failed: {Status} {Body}", (int)response.StatusCode, payload);
            return null;
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var sessionId = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var url = root.TryGetProperty("url", out var urlEl) ? urlEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(url))
        {
            logger.LogError("Stripe checkout response missing id or url. Body: {Body}", payload);
            return null;
        }
        return new SubscriptionCheckoutResult(sessionId!, url!, customerId);
    }

    private async Task<string?> ResolveOrCreateCustomerAsync(string secretKey, Guid tenantId, CancellationToken cancellationToken)
    {
        var existing = await tenantStripeLinkRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (existing is not null) return existing.StripeCustomerId;

        using var client = httpClientFactory.CreateClient("stripe-refunds");
        if (client.BaseAddress is null) client.BaseAddress = new Uri(configuration["STRIPE_API_BASE_URL"] ?? "https://api.stripe.com/");
        using var req = new HttpRequestMessage(HttpMethod.Post, "v1/customers");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["metadata[tenantId]"] = tenantId.ToString("D"),
            ["description"] = $"Voyara tenant {tenantId:D}",
        });
        using var res = await client.SendAsync(req, cancellationToken);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("Stripe customer create failed: {Status} {Body}", (int)res.StatusCode, body);
            return null;
        }
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(cancellationToken));
        var customerId = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrWhiteSpace(customerId)) return null;
        var link = TenantStripeLink.Create(tenantId, customerId);
        await tenantStripeLinkRepository.AddAsync(link, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return customerId;
    }
}
