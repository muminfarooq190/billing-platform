using BillingService.Application.Abstractions;
using BillingService.Domain.Enums;
using BillingService.Domain.ValueObjects;

namespace BillingService.Infrastructure.Payments;

public sealed class MockPaymentGateway(IConfiguration configuration) : IPaymentGateway
{
    public Task<PaymentResult> ProcessAsync(Guid invoiceId, Money amount, CancellationToken cancellationToken)
    {
        var mode = configuration["PAYMENT_GATEWAY_MODE"] ?? "success";
        var result = mode.ToLowerInvariant() switch
        {
            "declined" => PaymentResult.Declined,
            "insufficient" => PaymentResult.InsufficientFunds,
            "error" => PaymentResult.GatewayError,
            _ => PaymentResult.Success
        };

        return Task.FromResult(result);
    }
}
