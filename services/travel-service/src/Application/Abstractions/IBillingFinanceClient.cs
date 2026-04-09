namespace TravelService.Application.Abstractions;

public interface IBillingFinanceClient
{
    Task<IReadOnlyList<BookingInvoiceDto>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken);
}

public sealed record BookingInvoiceDto(
    Guid Id,
    Guid TenantId,
    string Status,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset DueDate,
    DateTimeOffset? PaidAt);
