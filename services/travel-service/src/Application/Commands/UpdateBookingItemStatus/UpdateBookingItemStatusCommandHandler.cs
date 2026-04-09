using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateBookingItemStatus;

public sealed class UpdateBookingItemStatusCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IAuditWriter auditWriter,
    IActorContext actorContext,
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

        var previousStatus = item.Status;
        item.UpdateStatus(request.Status);
        await bookingItemRepository.UpdateAsync(item, cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                item.TenantId,
                "BookingItem",
                item.Id,
                "StatusChanged",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                before: new { Status = previousStatus },
                after: new { Status = item.Status },
                metadata: new { item.BookingId, item.Title, request.Status }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
