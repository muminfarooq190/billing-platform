using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingPayments.UpdateBookingPayment;

public sealed class UpdateBookingPaymentCommandHandler(IBookingPaymentRepository bookingPaymentRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookingPaymentCommand>
{
    public async Task Handle(UpdateBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await bookingPaymentRepository.GetByIdAsync(request.PaymentId, cancellationToken) ?? throw new InvalidOperationException("Payment not found.");
        if (payment.TenantId != request.TenantId || payment.BookingId != request.BookingId)
            throw new InvalidOperationException("Payment does not belong to booking.");

        payment.Update(request.MilestoneLabel, request.DueDate, request.Amount, request.Currency, request.Notes);
        await bookingPaymentRepository.UpdateAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
