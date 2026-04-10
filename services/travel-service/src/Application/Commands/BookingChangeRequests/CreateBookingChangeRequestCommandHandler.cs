using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingChangeRequests;

public sealed class CreateBookingChangeRequestCommandHandler(IBookingRepository bookingRepository, IBookingChangeRequestRepository changeRequestRepository, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingChangeRequestCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingChangeRequestCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");
        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var changeRequest = BookingChangeRequest.Create(booking.Id, booking.TenantId, request.ChangeType, request.Reason, actorContext.UserId);
        await changeRequestRepository.AddAsync(changeRequest, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(booking.TenantId, "Booking", booking.Id, "Updated", $"Booking change requested: {request.ChangeType}", new { changeRequest.Id, changeRequest.ChangeType, changeRequest.Reason }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return changeRequest.Id;
    }
}
