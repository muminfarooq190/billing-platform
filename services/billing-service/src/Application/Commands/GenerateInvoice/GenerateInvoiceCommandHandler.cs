using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using BillingService.Domain.ValueObjects;
using MediatR;

namespace BillingService.Application.Commands.GenerateInvoice;

public sealed class GenerateInvoiceCommandHandler(ISubscriptionRepository subscriptionRepository, IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork, ICacheService cacheService) : IRequestHandler<GenerateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken) ?? throw new InvalidOperationException("Subscription not found.");
        var lineItems = new[] { new InvoiceLineItem("Base plan", 1, new Money(49.0000m, "USD")) };
        var invoice = Invoice.Generate(subscription.Id, subscription.TenantId, lineItems, new Money(4.9000m, "USD"), DateTimeOffset.UtcNow.AddDays(14));
        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync($"billing:dashboard:{subscription.TenantId}", cancellationToken);
        return invoice.Id;
    }
}
