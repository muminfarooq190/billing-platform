using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateBookingItem;

public sealed class UpdateBookingItemCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookingItemCommand>
{
    public async Task Handle(UpdateBookingItemCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var item = await bookingItemRepository.GetByIdAsync(request.BookingId, request.ItemId, cancellationToken)
            ?? throw new DomainException("Booking item not found.");

        if (item.TenantId != request.TenantId)
            throw new DomainException("Booking item does not belong to the active tenant.");

        item.Update(
            request.Type,
            request.SupplierName,
            request.SupplierReference,
            request.Title,
            request.Description,
            request.Location,
            request.StartAt,
            request.EndAt,
            request.SellAmount,
            request.CostAmount,
            request.Currency,
            request.VoucherNumber,
            request.ConfirmationNumber,
            request.AssignedToUserId,
            request.Notes,
            request.SortOrder);

        await bookingItemRepository.UpdateAsync(item, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Booking",
                booking.Id,
                "Updated",
                $"Booking item updated: {item.Title}",
                new { item.Id, item.Type, item.Status, item.SupplierName, item.StartAt, item.EndAt },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
