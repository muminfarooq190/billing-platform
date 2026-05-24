using System.Text.Json;
using BillingService.Application.Abstractions;
using BillingService.Application.Commands.ProcessStripeWebhook;
using BillingService.Infrastructure.Payments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/webhooks/stripe")]
public sealed class StripeWebhooksController(
    IMediator mediator,
    IConfiguration configuration,
    IStripeWebhookVerifier stripeWebhookVerifier,
    ICacheService cache,
    IWebHostEnvironment env,
    ILogger<StripeWebhooksController> logger) : ControllerBase
{
    /// <summary>
    /// Dedup window for Stripe event ids. Stripe retries failed webhooks for
    /// up to 3 days, so we keep ids for 7 days as a safety margin. Storing in
    /// Redis (via ICacheService) avoids a migration; for tamper-proof audit
    /// move to a `stripe_processed_events` table later.
    /// </summary>
    private static readonly TimeSpan DedupTtl = TimeSpan.FromDays(7);

    /// <summary>
    /// Short-lived "in-flight" lock TTL. Prevents two parallel webhook
    /// invocations from processing the same event simultaneously while the
    /// first is still running. Long enough to cover the slowest handler
    /// (current ceiling ~30s for Stripe → Postgres + cache busts).
    /// </summary>
    private static readonly TimeSpan InFlightTtl = TimeSpan.FromMinutes(2);
    private const string DedupKeyPrefix = "stripe-evt:";
    private const string InFlightKeyPrefix = "stripe-evt-inflight:";

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return BadRequest(new { error = "Empty webhook payload." });

        var webhookSecret = configuration["STRIPE_WEBHOOK_SECRET"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            // Production-grade safety: never accept an unsigned webhook in a
            // non-Development environment. In Dev we allow missing secret so
            // local Stripe CLI can hit the endpoint without ceremony.
            if (!env.IsDevelopment())
            {
                logger.LogError("Stripe webhook hit without STRIPE_WEBHOOK_SECRET configured in {Env}", env.EnvironmentName);
                return StatusCode(503, new { error = "Stripe webhook secret not configured on this environment." });
            }
            logger.LogWarning("STRIPE_WEBHOOK_SECRET missing — accepting webhook unverified (Development only).");
        }
        else
        {
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            if (!stripeWebhookVerifier.IsValid(payload, signatureHeader, webhookSecret, out var failureReason))
                return Unauthorized(new { error = failureReason });
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (!root.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            return BadRequest(new { error = "Stripe webhook payload must include a top-level type." });

        // -- Idempotency by Stripe event id ------------------------------------
        // Stripe delivers at-least-once. Same event id arriving twice MUST be
        // treated as a no-op (Stripe expects 2xx so it stops retrying).
        //
        // Two-key scheme:
        //   stripe-evt:{id}          — long-lived "succeeded" marker (7d TTL).
        //                              Set ONLY after mediator returns success.
        //   stripe-evt-inflight:{id} — short-lived processing lock (2m TTL).
        //                              Prevents parallel-replay re-processing.
        //
        // Previous design stamped the long marker BEFORE the mediator ran. If
        // the mediator threw, the event stayed dedup-locked for 7 days while
        // never having taken effect — Stripe's retries returned `duplicate=true`
        // and the invoice silently stayed unpaid. Now: lock at the start
        // (short TTL), commit the long marker only on success. Mediator throw
        // → lock auto-expires in 2m → Stripe's next retry succeeds.
        var stripeEventId = root.TryGetProperty("id", out var idElement) && idElement.ValueKind == JsonValueKind.String
            ? idElement.GetString()
            : null;

        if (!string.IsNullOrWhiteSpace(stripeEventId))
        {
            var dedupKey = DedupKeyPrefix + stripeEventId;
            var inFlightKey = InFlightKeyPrefix + stripeEventId;
            var alreadySeen = await cache.GetAsync<DateTimeOffset?>(dedupKey, cancellationToken);
            if (alreadySeen.HasValue)
            {
                logger.LogInformation("Stripe webhook {EventId} replayed — acknowledging without re-processing.", stripeEventId);
                return Ok(new { duplicate = true, firstSeenAt = alreadySeen.Value });
            }
            var inFlight = await cache.GetAsync<DateTimeOffset?>(inFlightKey, cancellationToken);
            if (inFlight.HasValue)
            {
                // Parallel replay arrived while the first is still running.
                // Return 409 so Stripe retries after their standard backoff.
                logger.LogWarning("Stripe webhook {EventId} arrived while another worker is still processing it.", stripeEventId);
                return Conflict(new { error = "Event is currently being processed; retry shortly." });
            }
            await cache.SetAsync(inFlightKey, DateTimeOffset.UtcNow, InFlightTtl, cancellationToken);
        }
        else
        {
            logger.LogWarning("Stripe webhook missing top-level id — proceeding without idempotency guard.");
        }

        if (!root.TryGetProperty("data", out var dataElement)
            || !dataElement.TryGetProperty("object", out var objectElement)
            || objectElement.ValueKind != JsonValueKind.Object)
            return BadRequest(new { error = "Stripe webhook payload must include data.object." });

        if (!TryResolveInvoiceId(objectElement, out var invoiceId))
            return BadRequest(new { error = "Stripe webhook payload did not contain a resolvable invoice id in metadata.invoiceId or invoice metadata." });

        var providerPaymentId = ResolveString(objectElement, "payment_intent")
            ?? ResolveString(objectElement, "id");
        var errorCode = objectElement.TryGetProperty("last_payment_error", out var lastPaymentError)
            ? ResolveString(lastPaymentError, "code")
            : null;
        var errorMessage = objectElement.TryGetProperty("last_payment_error", out lastPaymentError)
            ? ResolveString(lastPaymentError, "message")
            : null;

        // `charge.refunded` payload puts the refund inside refunds.data[]. Pick
        // the most-recently-created one; for full refunds there's only one row.
        var refundId = TryExtractMostRecentRefundId(objectElement);

        string result;
        try
        {
            result = await mediator.Send(new ProcessStripeWebhookCommand(
                typeElement.GetString()!,
                invoiceId,
                providerPaymentId,
                errorCode,
                errorMessage,
                refundId), cancellationToken);
        }
        catch
        {
            // Mediator failure → drop the in-flight lock so Stripe's next
            // retry can pick up cleanly instead of hitting "Conflict" until
            // the 2-minute lock window expires.
            if (!string.IsNullOrWhiteSpace(stripeEventId))
            {
                await cache.RemoveAsync(InFlightKeyPrefix + stripeEventId, cancellationToken);
            }
            throw;
        }

        // Stamp the long-lived "succeeded" marker only after the mediator
        // returned. Clear the in-flight lock — its job is done.
        if (!string.IsNullOrWhiteSpace(stripeEventId))
        {
            await cache.SetAsync(DedupKeyPrefix + stripeEventId, DateTimeOffset.UtcNow, DedupTtl, cancellationToken);
            await cache.RemoveAsync(InFlightKeyPrefix + stripeEventId, cancellationToken);
        }

        return Ok(new { result });
    }

    private static string? TryExtractMostRecentRefundId(JsonElement chargeOrRefund)
    {
        // Direct refund payload (charge.refund.updated): object IS the refund.
        if (chargeOrRefund.TryGetProperty("object", out var objKind)
            && objKind.ValueKind == JsonValueKind.String
            && string.Equals(objKind.GetString(), "refund", StringComparison.OrdinalIgnoreCase))
        {
            return ResolveString(chargeOrRefund, "id");
        }
        // charge.refunded: charge object w/ nested refunds.data[]
        if (!chargeOrRefund.TryGetProperty("refunds", out var refunds)) return null;
        if (!refunds.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return null;
        string? mostRecent = null;
        long mostRecentCreated = long.MinValue;
        foreach (var refund in data.EnumerateArray())
        {
            var id = ResolveString(refund, "id");
            var createdRaw = ResolveString(refund, "created");
            if (long.TryParse(createdRaw, out var created) && created > mostRecentCreated)
            {
                mostRecent = id;
                mostRecentCreated = created;
            }
            else if (mostRecent is null)
            {
                mostRecent = id; // fallback when `created` missing
            }
        }
        return mostRecent;
    }

    private static bool TryResolveInvoiceId(JsonElement objectElement, out Guid invoiceId)
    {
        invoiceId = Guid.Empty;

        if (TryResolveInvoiceIdFromMetadata(objectElement, out invoiceId))
            return true;

        if (objectElement.TryGetProperty("invoice", out var invoiceElement) && invoiceElement.ValueKind == JsonValueKind.Object)
        {
            if (TryResolveInvoiceIdFromMetadata(invoiceElement, out invoiceId))
                return true;
        }

        return false;
    }

    private static bool TryResolveInvoiceIdFromMetadata(JsonElement element, out Guid invoiceId)
    {
        invoiceId = Guid.Empty;
        if (!element.TryGetProperty("metadata", out var metadataElement) || metadataElement.ValueKind != JsonValueKind.Object)
            return false;

        var invoiceIdRaw = ResolveString(metadataElement, "invoiceId") ?? ResolveString(metadataElement, "invoice_id");
        return Guid.TryParse(invoiceIdRaw, out invoiceId);
    }

    private static string? ResolveString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }
}
