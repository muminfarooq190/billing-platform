using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingFulfillment;

public sealed class ConfirmBookingItemCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmBookingItemCommand>
{
    public async Task Handle(ConfirmBookingItemCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");
        var item = await bookingItemRepository.GetByIdAsync(request.BookingId, request.ItemId, cancellationToken)
            ?? throw new DomainException($"Booking item {request.ItemId} not found.");

        if (booking.TenantId != request.TenantId || item.TenantId != request.TenantId)
            throw new DomainException("Booking item does not belong to the active tenant.");

        item.Confirm(request.ConfirmationNumber, request.ConfirmedAt, request.Notes);
        await bookingItemRepository.UpdateAsync(item, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(request.TenantId, "Booking", booking.Id, "StatusChanged", $"Booking item confirmed: {item.Title}", new { item.Id, item.Status, item.ConfirmationNumber, item.ConfirmedAt }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
