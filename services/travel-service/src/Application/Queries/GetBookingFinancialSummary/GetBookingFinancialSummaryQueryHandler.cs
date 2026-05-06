using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.GetBookingFinancialSummary;

public sealed class GetBookingFinancialSummaryQueryHandler(IBookingRepository bookingRepository, IBookingPaymentRepository bookingPaymentRepository, IFeatureGate featureGate) : IRequestHandler<GetBookingFinancialSummaryQuery, BookingFinancialSummaryReadModel?>
{
    public async Task<BookingFinancialSummaryReadModel?> Handle(GetBookingFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelAuditRead, request.TenantId, cancellationToken);

        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null || booking.TenantId != request.TenantId)
            return null;

        var payments = await bookingPaymentRepository.ListByBookingIdAsync(request.BookingId, cancellationToken);
        var paidPayments = payments.Where(x => x.Status == BookingPaymentStatus.Paid).ToList();
        var outstandingPayments = payments.Where(x => x.Status is BookingPaymentStatus.Scheduled or BookingPaymentStatus.Pending).ToList();
        var paidAmount = paidPayments.Sum(x => x.Amount);
        var outstandingAmount = Math.Max(booking.TotalSellAmount - paidAmount, 0m);
        var nextPaymentDueAt = outstandingPayments
            .OrderBy(x => x.DueDate)
            .Select(x => (DateTimeOffset?)x.DueDate)
            .FirstOrDefault();
        var lastPaymentAt = paidPayments
            .Where(x => x.PaidAt.HasValue)
            .OrderByDescending(x => x.PaidAt)
            .Select(x => x.PaidAt)
            .FirstOrDefault();

        var paymentStatus = outstandingAmount <= 0m
            ? "Paid"
            : paidAmount > 0m
                ? "PartiallyPaid"
                : outstandingPayments.Any(x => x.DueDate < DateTimeOffset.UtcNow)
                    ? "Overdue"
                    : payments.Count == 0
                        ? "Unscheduled"
                        : "Pending";

        return new BookingFinancialSummaryReadModel(
            booking.Id,
            booking.Currency,
            booking.TotalSellAmount,
            paidAmount,
            outstandingAmount,
            paymentStatus,
            nextPaymentDueAt,
            lastPaymentAt,
            payments.Count);
    }
}
