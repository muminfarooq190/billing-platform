namespace TravelService.Application.Queries.BookingPayments;

public sealed record BookingPaymentReadModel(
    Guid Id,
    Guid BookingId,
    string? MilestoneLabel,
    DateTimeOffset DueDate,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset? PaidAt,
    string? PaymentMethod,
    string? ProviderReference,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
