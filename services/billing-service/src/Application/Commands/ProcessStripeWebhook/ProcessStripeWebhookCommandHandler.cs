using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.ProcessStripeWebhook;

public sealed class ProcessStripeWebhookCommandHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<ProcessStripeWebhookCommand, string>
{
    public async Task<string> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        if (string.Equals(request.EventType, "payment_intent.succeeded", StringComparison.OrdinalIgnoreCase)
            || string.Equals(request.EventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkAsPaid(DateTimeOffset.UtcNow, "Stripe", request.ProviderPaymentId);
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"billing:invoice:{invoice.Id}", cancellationToken);
            await cacheService.RemoveAsync($"billing:dashboard:{invoice.TenantId}", cancellationToken);
            return "Paid";
        }

        if (string.Equals(request.EventType, "payment_intent.payment_failed", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkPaymentFailed("Stripe", request.ErrorCode, request.ErrorMessage ?? "Stripe reported payment failure.");
            invoice.MarkOverdue();
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return "Failed";
        }

        return "Ignored";
    }
}
