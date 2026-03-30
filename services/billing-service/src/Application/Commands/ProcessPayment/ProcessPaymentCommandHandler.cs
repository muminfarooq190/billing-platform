using BillingService.Application.Abstractions;
using BillingService.Domain.Enums;
using BillingService.Domain.Events;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.ProcessPayment;

public sealed class ProcessPaymentCommandHandler(IInvoiceRepository invoiceRepository, IPaymentGateway paymentGateway, IUnitOfWork unitOfWork, ICacheService cacheService) : IRequestHandler<ProcessPaymentCommand, string>
{
    public async Task<string> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken) ?? throw new InvalidOperationException("Invoice not found.");
        var result = await paymentGateway.ProcessAsync(invoice.Id, invoice.Total, cancellationToken);

        if (result == PaymentResult.Success)
        {
            invoice.MarkAsPaid(DateTimeOffset.UtcNow);
            await invoiceRepository.UpdateAsync(invoice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveAsync($"billing:invoice:{invoice.Id}", cancellationToken);
            await cacheService.RemoveAsync($"billing:dashboard:{invoice.TenantId}", cancellationToken);
            return "Success";
        }

        invoice.MarkOverdue();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result.ToString();
    }
}
