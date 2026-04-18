using System.Text.Json;
using BillingService.Application.Commands.ProcessStripeWebhook;
using BillingService.Infrastructure.Payments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/webhooks/stripe")]
public sealed class StripeWebhooksController(IMediator mediator, IConfiguration configuration, IStripeWebhookVerifier stripeWebhookVerifier) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
            return BadRequest(new { error = "Empty webhook payload." });

        var webhookSecret = configuration["STRIPE_WEBHOOK_SECRET"];
        if (!string.IsNullOrWhiteSpace(webhookSecret))
        {
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
            if (!stripeWebhookVerifier.IsValid(payload, signatureHeader, webhookSecret, out var failureReason))
                return Unauthorized(new { error = failureReason });
        }

        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        if (!root.TryGetProperty("type", out var typeElement) || typeElement.ValueKind != JsonValueKind.String)
            return BadRequest(new { error = "Stripe webhook payload must include a top-level type." });

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

        var result = await mediator.Send(new ProcessStripeWebhookCommand(
            typeElement.GetString()!,
            invoiceId,
            providerPaymentId,
            errorCode,
            errorMessage), cancellationToken);

        return Ok(new { result });
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
