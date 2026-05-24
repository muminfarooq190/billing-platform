using System.Net.Http.Headers;
using System.Text.Json;
using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

/// <summary>
/// PaymentIntent endpoint for embedded Stripe Elements checkout.
///
/// Flow (caller = Nexus or portal):
///   1. POST /billing/payment-intents { invoiceId? | amount + currency, tenantId }
///   2. Server lazy-creates Stripe Customer for tenant (reuses TenantStripeLink)
///   3. Server calls Stripe POST /v1/payment_intents with
///      automatic_payment_methods=enabled, customer=cus_*
///   4. Returns { clientSecret, paymentIntentId }
///   5. Client renders Stripe Elements + calls stripe.confirmPayment(clientSecret)
///   6. Stripe webhook payment_intent.succeeded → existing handler flips invoice
///
/// Why this exists vs the Checkout Session in StripePaymentGateway:
///   Checkout Session redirects the user to Stripe's hosted page. Elements
///   keeps the user on our domain — required for the embedded Nexus checkout
///   step.
/// </summary>
[ApiController]
[Route("billing/payment-intents")]
public sealed class PaymentIntentsController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ITenantStripeLinkRepository tenantStripeLinkRepository,
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentIntentsController> logger) : ControllerBase
{
    public sealed record PaymentIntentRequest(
        Guid TenantId,
        Guid? InvoiceId,
        decimal? Amount,
        string? Currency,
        string? Description);

    public sealed record PaymentIntentResponse(
        string PaymentIntentId,
        string ClientSecret,
        string Status,
        long AmountMinor,
        string Currency,
        string CustomerId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PaymentIntentRequest request, CancellationToken cancellationToken)
    {
        var secretKey = configuration["STRIPE_SECRET_KEY"];
        if (string.IsNullOrWhiteSpace(secretKey))
            return StatusCode(503, new { error = "Stripe is not configured." });

        if (request.TenantId == Guid.Empty)
            return BadRequest(new { error = "TenantId is required." });

        // Resolve amount + currency either from referenced invoice or
        // explicit request fields. Invoice path is authoritative when both
        // present (UI can't tamper with billed amounts).
        decimal amount;
        string currency;
        Guid? invoiceId = request.InvoiceId;

        if (invoiceId.HasValue && invoiceId.Value != Guid.Empty)
        {
            var invoice = await invoiceRepository.GetByIdAsync(invoiceId.Value, cancellationToken);
            if (invoice is null) return NotFound(new { error = "Invoice not found." });
            if (invoice.TenantId != request.TenantId)
                return BadRequest(new { error = "Invoice does not belong to tenant." });
            amount = invoice.Total.Amount;
            currency = invoice.Total.Currency;
        }
        else
        {
            if (!(request.Amount is { } amt) || amt <= 0m)
                return BadRequest(new { error = "Amount must be positive when InvoiceId is omitted." });
            if (string.IsNullOrWhiteSpace(request.Currency))
                return BadRequest(new { error = "Currency is required when InvoiceId is omitted." });
            amount = amt;
            currency = request.Currency!;
        }

        // -- Resolve or lazy-create Stripe Customer for tenant ----------------
        var customerId = await ResolveCustomerIdAsync(secretKey, request.TenantId, cancellationToken);
        if (customerId is null)
            return StatusCode(502, new { error = "Could not resolve Stripe customer for tenant." });

        // -- Create PaymentIntent ---------------------------------------------
        var minorUnits = Convert.ToInt64(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
        var form = new Dictionary<string, string>
        {
            ["amount"] = minorUnits.ToString(),
            ["currency"] = currency.ToLowerInvariant(),
            ["customer"] = customerId,
            ["automatic_payment_methods[enabled]"] = "true",
            ["metadata[tenantId]"] = request.TenantId.ToString("D"),
        };
        if (invoiceId.HasValue)
            form["metadata[invoiceId]"] = invoiceId.Value.ToString("D");
        if (!string.IsNullOrWhiteSpace(request.Description))
            form["description"] = request.Description!;

        using var client = httpClientFactory.CreateClient("stripe-refunds");
        if (client.BaseAddress is null) client.BaseAddress = new Uri(configuration["STRIPE_API_BASE_URL"] ?? "https://api.stripe.com/");
        using var stripeRequest = new HttpRequestMessage(HttpMethod.Post, "v1/payment_intents");
        stripeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);
        // Idempotency: same invoice + tenant + amount within a short window
        // should resolve to the same PaymentIntent. Without it, double-clicks
        // on the checkout button create duplicate PaymentIntents.
        stripeRequest.Headers.Add("Idempotency-Key",
            $"pi:{request.TenantId:D}:{invoiceId?.ToString("D") ?? "ad-hoc"}:{minorUnits}:{currency.ToLowerInvariant()}");
        stripeRequest.Content = new FormUrlEncodedContent(form);

        using var stripeResponse = await client.SendAsync(stripeRequest, cancellationToken);
        var payload = await stripeResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!stripeResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("Stripe PaymentIntent create failed: {Status} {Body}", (int)stripeResponse.StatusCode, payload);
            return StatusCode((int)stripeResponse.StatusCode, new { error = "Stripe rejected the payment intent.", stripe = payload });
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        var pid = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var clientSecret = root.TryGetProperty("client_secret", out var csEl) ? csEl.GetString() : null;
        var status = root.TryGetProperty("status", out var stEl) ? stEl.GetString() : null;

        if (string.IsNullOrWhiteSpace(pid) || string.IsNullOrWhiteSpace(clientSecret))
            return StatusCode(502, new { error = "Stripe PaymentIntent response missing id or client_secret." });

        return Ok(new PaymentIntentResponse(pid!, clientSecret!, status ?? "requires_payment_method", minorUnits, currency.ToUpperInvariant(), customerId));
    }

    private async Task<string?> ResolveCustomerIdAsync(string secretKey, Guid tenantId, CancellationToken cancellationToken)
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
