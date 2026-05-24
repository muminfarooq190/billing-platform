using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.ProcessStripeWebhook;

/// <summary>
/// Dispatches Stripe webhook events to the right Invoice aggregate
/// transition. The controller does signature verification + idempotency
/// before this handler runs; the handler is pure aggregate orchestration.
///
/// Event coverage:
///   payment_intent.succeeded / checkout.session.completed
///       → MarkAsPaid, cache busts
///   invoice.payment_succeeded (Stripe-native recurring)
///       → MarkAsPaid (same handling, separate event name)
///   payment_intent.payment_failed
///       → MarkPaymentFailed + MarkOverdue
///   payment_intent.requires_action
///       → stamp failure metadata so the UI can prompt 3DS without
///         transitioning to Failed/Paid
///   charge.refunded
///       → MarkRefunded (fires InvoiceRefundedEvent)
///   anything else → "Ignored"
///
/// Subscription lifecycle events (customer.subscription.*) require
/// Stripe-native subscription-mode billing — not modeled today.
/// </summary>
public sealed class ProcessStripeWebhookCommandHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<ProcessStripeWebhookCommand, string>
{
    public async Task<string> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        var eventType = request.EventType?.Trim() ?? string.Empty;

        // -- Paid ---------------------------------------------------------------
        if (string.Equals(eventType, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "invoice.payment_succeeded", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkAsPaid(DateTimeOffset.UtcNow, "Stripe", request.ProviderPaymentId);
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"billing:invoice:{invoice.Id}", cancellationToken);
            await cacheService.RemoveAsync($"billing:dashboard:{invoice.TenantId}", cancellationToken);
            return "Paid";
        }

        // -- Failed -------------------------------------------------------------
        if (string.Equals(eventType, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "invoice.payment_failed", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkPaymentFailed("Stripe", request.ErrorCode, request.ErrorMessage ?? "Stripe reported payment failure.");
            invoice.MarkOverdue();
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return "Failed";
        }

        // -- Requires action (3DS / SCA) ---------------------------------------
        // Don't transition status — payment is still mid-flight. Just stamp
        // the failure metadata so the frontend can show "Verify card" and
        // surface the next_action URL provided by Stripe.
        if (string.Equals(eventType, "payment_intent.requires_action", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "payment_intent.action_required", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkPaymentFailed(
                "Stripe",
                request.ErrorCode ?? "requires_action",
                request.ErrorMessage ?? "Customer must complete 3-D Secure authentication.");
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return "RequiresAction";
        }

        // -- Refunded -----------------------------------------------------------
        if (string.Equals(eventType, "charge.refunded", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventType, "charge.refund.updated", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkRefunded(DateTimeOffset.UtcNow, "Stripe", request.RefundId ?? request.ProviderPaymentId);
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"billing:invoice:{invoice.Id}", cancellationToken);
            await cacheService.RemoveAsync($"billing:dashboard:{invoice.TenantId}", cancellationToken);
            return "Refunded";
        }

        return "Ignored";
    }
}
