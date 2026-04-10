using MediatR;

namespace TravelService.Application.Queries.GetBookingFinancialSummary;

public sealed record GetBookingFinancialSummaryQuery(Guid TenantId, Guid BookingId) : IRequest<BookingFinancialSummaryReadModel?>;

public sealed record BookingFinancialSummaryReadModel(
    Guid BookingId,
    string Currency,
    decimal BookingTotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    string PaymentStatus,
    DateTimeOffset? NextPaymentDueAt,
    DateTimeOffset? LastPaymentAt,
    int InvoiceCount);
