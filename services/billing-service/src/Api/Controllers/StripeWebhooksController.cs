using BillingService.Api.Contracts;
using BillingService.Application.Commands.ProcessStripeWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("billing/webhooks/stripe")]
public sealed class StripeWebhooksController(IMediator mediator, IConfiguration configuration) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] StripeWebhookRequest request, CancellationToken cancellationToken)
    {
        var expectedSignature = configuration["STRIPE_WEBHOOK_SECRET"];
        if (!string.IsNullOrWhiteSpace(expectedSignature) && !string.Equals(expectedSignature, request.Signature, StringComparison.Ordinal))
            return Unauthorized();

        var result = await mediator.Send(new ProcessStripeWebhookCommand(
            request.EventType,
            Guid.Parse(request.InvoiceId),
            request.ProviderPaymentId,
            request.ErrorCode,
            request.ErrorMessage), cancellationToken);

        return Ok(new { result });
    }
}
