using MediatR;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Queries.BookingPayments;

public sealed class ListBookingPaymentsQueryHandler(IBookingRepository bookingRepository, IBookingPaymentRepository bookingPaymentRepository) : IRequestHandler<ListBookingPaymentsQuery, IReadOnlyList<BookingPaymentReadModel>>
{
    public async Task<IReadOnlyList<BookingPaymentReadModel>> Handle(ListBookingPaymentsQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null || booking.TenantId != request.TenantId)
            return [];

        var payments = await bookingPaymentRepository.ListByBookingIdAsync(request.BookingId, cancellationToken);
        return payments.Select(x => new BookingPaymentReadModel(
            x.Id,
            x.BookingId,
            x.MilestoneLabel,
            x.DueDate,
            x.Amount,
            x.Currency,
            x.Status.ToString(),
            x.PaidAt,
            x.PaymentMethod,
            x.ProviderReference,
            x.Notes,
            x.CreatedAt,
            x.UpdatedAt)).ToList();
    }
}
