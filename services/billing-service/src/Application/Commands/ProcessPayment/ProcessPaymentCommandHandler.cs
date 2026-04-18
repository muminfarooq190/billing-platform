using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler(
    IInvoiceRepository invoiceRepository,
    IPaymentGateway paymentGateway,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<ProcessPaymentCommand, string>
{
    public async Task<string> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found.");

        var result = await paymentGateway.ProcessAsync(invoice.Id, invoice.TenantId, invoice.Total, cancellationToken);

        if (string.Equals(result.Status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkAsPaid(DateTimeOffset.UtcNow, result.Gateway, result.ProviderPaymentId);
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"billing:invoice:{invoice.Id}", cancellationToken);
            await cacheService.RemoveAsync($"billing:dashboard:{invoice.TenantId}", cancellationToken);
            return "Success";
        }

        if (string.Equals(result.Status, "RequiresAction", StringComparison.OrdinalIgnoreCase))
        {
            invoice.MarkPaymentPending(result.Gateway, result.ProviderPaymentId ?? invoice.Id.ToString("N"));
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return $"ActionRequired:{result.CheckoutUrl}";
        }

        invoice.MarkPaymentFailed(result.Gateway, result.ErrorCode, result.ErrorMessage);
        invoice.MarkOverdue();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return $"Failed:{result.ErrorCode ?? "gateway_error"}";
    }
}
