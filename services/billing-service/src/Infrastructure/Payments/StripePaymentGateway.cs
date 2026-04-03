using BillingService.Application.Abstractions;
using BillingService.Domain.Enums;
using BillingService.Domain.ValueObjects;

namespace BillingService.Infrastructure.Payments;

public sealed class StripePaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ProcessAsync(Guid invoiceId, Money amount, CancellationToken cancellationToken)
    {
        return Task.FromResult(PaymentResult.Success);
    }
}
