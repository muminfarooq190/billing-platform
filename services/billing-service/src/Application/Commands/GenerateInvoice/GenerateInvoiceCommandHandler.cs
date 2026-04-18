using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.GenerateInvoice;

public sealed class GenerateInvoiceCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IInvoiceRepository invoiceRepository,
    IBillingPricingResolver billingPricingResolver,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<GenerateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        var pricing = await billingPricingResolver.ResolveAsync(subscription, cancellationToken);
        var existing = await invoiceRepository.GetBySubscriptionAndBillingPeriodAsync(subscription.Id, pricing.BillingPeriodStart, pricing.BillingPeriodEnd, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var invoice = Invoice.Generate(
            subscription.Id,
            subscription.TenantId,
            pricing.LineItems,
            pricing.TaxAmount,
            DateTimeOffset.UtcNow.AddDays(14),
            pricing.BillingPeriodStart,
            pricing.BillingPeriodEnd,
            pricing.PricingReference);

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheService.RemoveAsync($"billing:dashboard:{subscription.TenantId}", cancellationToken);
        return invoice.Id;
    }
}
