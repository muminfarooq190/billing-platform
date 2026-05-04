namespace BillingService.Application.ReadModels;

public sealed class InvoiceReadModel
{
    public Guid Id { get; init; }
    public Guid SubscriptionId { get; init; }
    public Guid TenantId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal? TotalAmount { get; init; }
    public decimal? PaidAmount { get; init; }
    public decimal? DueAmount { get; init; }
    public string? Currency { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
}
