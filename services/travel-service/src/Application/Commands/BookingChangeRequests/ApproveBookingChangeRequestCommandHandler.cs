using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingChangeRequests;

public sealed class ApproveBookingChangeRequestCommandHandler(IBookingRepository bookingRepository, IBookingChangeRequestRepository changeRequestRepository, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork) : IRequestHandler<ApproveBookingChangeRequestCommand>
{
    public async Task Handle(ApproveBookingChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");
        var changeRequest = await changeRequestRepository.GetByIdAsync(request.BookingId, request.ChangeRequestId, cancellationToken)
            ?? throw new DomainException("Booking change request not found.");
        if (booking.TenantId != request.TenantId || changeRequest.TenantId != request.TenantId)
            throw new DomainException("Booking change request does not belong to the active tenant.");

        changeRequest.Approve(actorContext.UserId, request.Reason);
        await changeRequestRepository.UpdateAsync(changeRequest, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(booking.TenantId, "Booking", booking.Id, "Updated", $"Booking change approved: {changeRequest.ChangeType}", new { changeRequest.Id, changeRequest.Status, changeRequest.DecisionReason }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
