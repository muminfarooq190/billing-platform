using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork) : IRequestHandler<MarkInvoiceOverdueCommand>
{
    public async Task Handle(MarkInvoiceOverdueCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken) ?? throw new InvalidOperationException("Invoice not found.");
        invoice.MarkOverdue();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
