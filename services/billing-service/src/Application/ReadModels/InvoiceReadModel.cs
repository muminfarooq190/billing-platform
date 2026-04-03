namespace BillingService.Application.ReadModels;

public sealed record InvoiceReadModel(Guid Id, Guid SubscriptionId, Guid TenantId, string Status, decimal TotalAmount, string Currency, DateTimeOffset DueDate, DateTimeOffset? PaidAt);
