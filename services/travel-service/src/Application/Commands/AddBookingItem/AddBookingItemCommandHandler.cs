using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.AddBookingItem;

public sealed class AddBookingItemCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<AddBookingItemCommand, Guid>
{
    public async Task<Guid> Handle(AddBookingItemCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var item = BookingItem.Create(
            request.BookingId,
            request.TenantId,
            request.Type,
            request.SupplierName,
            request.Title,
            request.Description,
            request.Location,
            request.StartAt,
            request.EndAt,
            request.SellAmount,
            request.CostAmount,
            request.Currency,
            request.Notes,
            request.SortOrder);

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

        await bookingItemRepository.AddAsync(item, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Booking",
                booking.Id,
                "Created",
                $"Booking item added: {item.Title}",
                new { item.Id, item.Type, item.Status, item.SupplierName, item.StartAt, item.EndAt },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return item.Id;
    }
}
