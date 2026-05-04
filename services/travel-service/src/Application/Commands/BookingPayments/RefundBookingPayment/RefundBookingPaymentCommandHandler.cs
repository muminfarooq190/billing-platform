using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingPayments.RefundBookingPayment;

public sealed class RefundBookingPaymentCommandHandler(IBookingRepository bookingRepository, IBookingPaymentRepository bookingPaymentRepository, IUnitOfWork unitOfWork, IActivityWriter activityWriter) : IRequestHandler<RefundBookingPaymentCommand>
{
    public async Task Handle(RefundBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken) ?? throw new InvalidOperationException("Booking not found.");
        var payment = await bookingPaymentRepository.GetByIdAsync(request.PaymentId, cancellationToken) ?? throw new InvalidOperationException("Payment not found.");
        if (booking.TenantId != request.TenantId || payment.TenantId != request.TenantId || payment.BookingId != booking.Id)
            throw new InvalidOperationException("Payment does not belong to booking.");

        payment.Refund(request.Notes, request.ActorUserId);
        await bookingPaymentRepository.UpdateAsync(payment, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(request.TenantId, "Booking", booking.Id, "PaymentRefunded", $"Payment refunded for {booking.BookingNumber}", new { payment.Id, payment.Amount, payment.Currency }, request.ActorUserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
