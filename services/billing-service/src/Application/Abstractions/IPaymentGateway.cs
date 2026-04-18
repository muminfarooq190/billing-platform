using BillingService.Domain.ValueObjects;

namespace BillingService.Application.Abstractions;

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ProcessAsync(Guid invoiceId, Guid tenantId, Money amount, CancellationToken cancellationToken);
}

public sealed record PaymentGatewayResult(
    string Status,
    string Gateway,
    string? ProviderPaymentId,
    string? CheckoutUrl,
    string? ClientSecret,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static PaymentGatewayResult RequiresAction(string gateway, string providerPaymentId, string checkoutUrl)
        => new("RequiresAction", gateway, providerPaymentId, checkoutUrl, null, null, null);

    public static PaymentGatewayResult Succeeded(string gateway, string providerPaymentId)
        => new("Succeeded", gateway, providerPaymentId, null, null, null, null);

    public static PaymentGatewayResult Failed(string gateway, string errorCode, string errorMessage)
        => new("Failed", gateway, null, null, null, errorCode, errorMessage);
}
