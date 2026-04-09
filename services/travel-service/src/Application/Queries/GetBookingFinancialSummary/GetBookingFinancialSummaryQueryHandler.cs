using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.GetBookingFinancialSummary;

public sealed class GetBookingFinancialSummaryQueryHandler(IBookingRepository bookingRepository, IBillingFinanceClient billingFinanceClient) : IRequestHandler<GetBookingFinancialSummaryQuery, BookingFinancialSummaryReadModel?>
{
    public async Task<BookingFinancialSummaryReadModel?> Handle(GetBookingFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null || booking.TenantId != request.TenantId)
            return null;

        var invoices = await billingFinanceClient.GetInvoicesAsync(request.TenantId, cancellationToken);
        var currency = booking.Currency;
        var paidInvoices = invoices.Where(x => string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase)).ToList();
        var paidAmount = paidInvoices.Sum(x => x.TotalAmount);
        var outstandingAmount = Math.Max(booking.TotalSellAmount - paidAmount, 0m);
        var nextPaymentDueAt = invoices
            .Where(x => !string.Equals(x.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.DueDate)
            .Select(x => (DateTimeOffset?)x.DueDate)
            .FirstOrDefault();
        var lastPaymentAt = paidInvoices
            .Where(x => x.PaidAt.HasValue)
            .OrderByDescending(x => x.PaidAt)
            .Select(x => x.PaidAt)
            .FirstOrDefault();

        var paymentStatus = outstandingAmount <= 0m
            ? "Paid"
            : paidAmount > 0m
                ? "PartiallyPaid"
                : invoices.Any(x => x.DueDate < DateTimeOffset.UtcNow)
                    ? "Overdue"
                    : "Pending";

        return new BookingFinancialSummaryReadModel(
            booking.Id,
            currency,
            booking.TotalSellAmount,
            paidAmount,
            outstandingAmount,
            paymentStatus,
            nextPaymentDueAt,
            lastPaymentAt,
            invoices.Count);
    }
}
