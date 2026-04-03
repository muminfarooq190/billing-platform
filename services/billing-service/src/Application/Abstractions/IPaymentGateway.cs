using BillingService.Domain.Enums;
using BillingService.Domain.ValueObjects;

namespace BillingService.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessAsync(Guid invoiceId, Money amount, CancellationToken cancellationToken);
}
