using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

/// <summary>
/// Cross-service refund endpoint. Currently only consumed by travel-service
/// when a tenant clicks "Refund" on a booking payment that was originally
/// captured through Stripe (PaymentMethod = "Stripe" + ProviderReference =
/// Stripe charge/payment_intent id).
///
/// Calls Stripe POST /v1/refunds directly using the platform STRIPE_SECRET_KEY.
/// Future: route through `IPaymentGateway.RefundAsync` once the abstraction
/// is generalized (today the gateway only handles charge-side calls).
/// </summary>
[ApiController]
[Route("billing/refunds")]
public sealed class RefundsController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<RefundsController> logger) : ControllerBase
{
    public sealed record RefundRequest(
        string ProviderReference,
        decimal? Amount,
        string? Currency,
        string? Reason,
        string? TenantId);

    public sealed record RefundResponse(string RefundId, string Status, string? Reason);

    [HttpPost]
    public async Task<IActionResult> Refund([FromBody] RefundRequest request, CancellationToken cancellationToken)
    {
        var secretKey = configuration["STRIPE_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secretKey))
            return StatusCode(503, new { error = "Stripe is not configured." });

        if (string.IsNullOrWhiteSpace(request.ProviderReference))
            return BadRequest(new { error = "ProviderReference is required (Stripe charge id `ch_*` or payment_intent `pi_*`)." });

        var form = new Dictionary<string, string>
        {
            // Stripe accepts either `charge` or `payment_intent` here. Caller
            // passes whichever they captured at payment time.
            [request.ProviderReference.StartsWith("pi_", StringComparison.OrdinalIgnoreCase) ? "payment_intent" : "charge"] = request.ProviderReference,
        };

        if (request.Amount is { } amt && amt > 0m)
        {
            var currency = (request.Currency ?? "usd").ToLowerInvariant();
            form["amount"] = Convert.ToInt64(decimal.Round(amt * 100m, 0, MidpointRounding.AwayFromZero)).ToString();
            // Stripe ignores `currency` on refunds — charge currency wins.
            // We include it as metadata for audit.
            form["metadata[currency]"] = currency;
        }

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            // Stripe whitelist: duplicate / fraudulent / requested_by_customer.
            // Anything else goes in metadata.
            var allowed = new[] { "duplicate", "fraudulent", "requested_by_customer" };
            if (allowed.Contains(request.Reason.ToLowerInvariant()))
                form["reason"] = request.Reason.ToLowerInvariant();
            else
                form["metadata[reason]"] = request.Reason;
        }

        if (!string.IsNullOrWhiteSpace(request.TenantId))
            form["metadata[tenantId]"] = request.TenantId;

        using var client = httpClientFactory.CreateClient("stripe-refunds");
        if (client.BaseAddress is null) client.BaseAddress = new Uri(configuration["STRIPE_API_BASE_URL"] ?? "https://api.stripe.com/");
        using var stripeRequest = new HttpRequestMessage(HttpMethod.Post, "v1/refunds");
        stripeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        // Stripe idempotency: same client + ProviderReference + amount should
        // never double-refund even on caller retry.
        stripeRequest.Headers.Add("Idempotency-Key", $"refund:{request.ProviderReference}:{request.Amount?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "full"}");
        stripeRequest.Content = new FormUrlEncodedContent(form);

        using var stripeResponse = await client.SendAsync(stripeRequest, cancellationToken);
        var payload = await stripeResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!stripeResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("Stripe refund failed: {Status} {Body}", (int)stripeResponse.StatusCode, payload);
            return StatusCode((int)stripeResponse.StatusCode, new { error = "Stripe refund failed.", stripe = payload });
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var refundId = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
        var failureReason = root.TryGetProperty("failure_reason", out var frEl) ? frEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(refundId))
            return StatusCode(502, new { error = "Stripe refund response missing id." });

        return Ok(new RefundResponse(refundId!, status ?? "unknown", failureReason));
    }
}
