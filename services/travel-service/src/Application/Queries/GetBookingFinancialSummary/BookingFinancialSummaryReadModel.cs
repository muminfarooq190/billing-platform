namespace TravelService.Application.Queries.GetBookingFinancialSummary;

public sealed record BookingFinancialSummaryReadModel(
    Guid BookingId,
    string Currency,
    decimal TotalSellAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string PaymentStatus,
    DateTimeOffset? NextPaymentDueAt,
    DateTimeOffset? LastPaymentAt,
    int InvoiceCount);
