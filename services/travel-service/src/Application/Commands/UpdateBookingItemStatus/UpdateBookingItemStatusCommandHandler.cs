using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateBookingItemStatus;

public sealed class UpdateBookingItemStatusCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookingItemStatusCommand>
{
    public async Task Handle(UpdateBookingItemStatusCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var item = await bookingItemRepository.GetByIdAsync(request.BookingId, request.ItemId, cancellationToken)
            ?? throw new DomainException("Booking item not found.");

        if (item.TenantId != request.TenantId)
            throw new DomainException("Booking item does not belong to the active tenant.");

        item.UpdateStatus(request.Status);
        await bookingItemRepository.UpdateAsync(item, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
