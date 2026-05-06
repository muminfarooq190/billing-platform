using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingPayments.ScheduleBookingPayment;

public sealed class ScheduleBookingPaymentCommandHandler(IBookingRepository bookingRepository, IBookingPaymentRepository bookingPaymentRepository, IUnitOfWork unitOfWork, IActivityWriter activityWriter) : IRequestHandler<ScheduleBookingPaymentCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleBookingPaymentCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken) ?? throw new InvalidOperationException("Booking not found.");
        if (booking.TenantId != request.TenantId)
            throw new InvalidOperationException("Booking does not belong to tenant.");

        var payment = BookingPayment.Schedule(request.TenantId, request.BookingId, request.MilestoneLabel, request.DueDate, request.Amount, request.Currency, request.ActorUserId, request.Notes);
        await bookingPaymentRepository.AddAsync(payment, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(request.TenantId, "Booking", booking.Id, "PaymentScheduled", $"Payment scheduled for {booking.BookingNumber}", new { payment.Id, payment.MilestoneLabel, payment.Amount, payment.Currency, payment.DueDate }, request.ActorUserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }
}
