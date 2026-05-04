using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingPayments.MarkBookingPaymentPaid;

public sealed class MarkBookingPaymentPaidCommandHandler(IBookingRepository bookingRepository, IBookingPaymentRepository bookingPaymentRepository, IUnitOfWork unitOfWork, IActivityWriter activityWriter) : IRequestHandler<MarkBookingPaymentPaidCommand>
{
    public async Task Handle(MarkBookingPaymentPaidCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken) ?? throw new InvalidOperationException("Booking not found.");
        var payment = await bookingPaymentRepository.GetByIdAsync(request.PaymentId, cancellationToken) ?? throw new InvalidOperationException("Payment not found.");
        if (booking.TenantId != request.TenantId || payment.TenantId != request.TenantId || payment.BookingId != booking.Id)
            throw new InvalidOperationException("Payment does not belong to booking.");

        payment.MarkPaid(request.PaidAt, request.PaymentMethod, request.ProviderReference, request.Notes, request.ActorUserId);
        await bookingPaymentRepository.UpdateAsync(payment, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(request.TenantId, "Booking", booking.Id, "PaymentMarkedPaid", $"Payment marked paid for {booking.BookingNumber}", new { payment.Id, payment.Amount, payment.Currency, payment.PaidAt, payment.PaymentMethod }, request.ActorUserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
