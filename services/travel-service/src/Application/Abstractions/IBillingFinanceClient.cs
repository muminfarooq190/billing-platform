namespace TravelService.Application.Abstractions;

public interface IBillingFinanceClient
{
    Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Issue a refund against the original Stripe charge / payment intent.
    /// Returns null when the gateway isn't Stripe-configured or the call
    /// failed for non-business reasons (network, 5xx) — caller falls back
    /// to offline-only refund and surfaces a warning.
    /// </summary>
    Task<BookingRefundResult?> RefundAsync(BookingRefundRequest request, CancellationToken cancellationToken);
}

public sealed record BookingInvoiceDto(
    Guid Id,
    Guid TenantId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset DueDate,
    DateTimeOffset? PaidAt);

public sealed record BookingRefundRequest(
    string ProviderReference,
    decimal? Amount,
    string? Currency,
    string? Reason,
    Guid TenantId);

public sealed record BookingRefundResult(string RefundId, string Status, string? FailureReason);
