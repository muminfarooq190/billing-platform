using BillingService.Application.Abstractions;
using BillingService.Domain.ValueObjects;

namespace BillingService.Infrastructure.Payments;

public sealed class MockPaymentGateway : IPaymentGateway
{
    public Task<PaymentGatewayResult> ProcessAsync(Guid invoiceId, Guid tenantId, Money amount, CancellationToken cancellationToken)
        => Task.FromResult(PaymentGatewayResult.Succeeded("Mock", $"mock_{invoiceId:N}"));
}
