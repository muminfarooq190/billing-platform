using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.BookingFulfillment;

public sealed class IssueBookingItemCommandHandler(
    IBookingRepository bookingRepository,
    IBookingItemRepository bookingItemRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<IssueBookingItemCommand>
{
    public async Task Handle(IssueBookingItemCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");
        var item = await bookingItemRepository.GetByIdAsync(request.BookingId, request.ItemId, cancellationToken)
            ?? throw new DomainException($"Booking item {request.ItemId} not found.");

        if (booking.TenantId != request.TenantId || item.TenantId != request.TenantId)
            throw new DomainException("Booking item does not belong to the active tenant.");

        item.Issue(request.VoucherNumber, request.IssuedAt, request.Notes);
        await bookingItemRepository.UpdateAsync(item, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(request.TenantId, "Booking", booking.Id, "StatusChanged", $"Booking item issued: {item.Title}", new { item.Id, item.Status, item.VoucherNumber, item.IssuedAt }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
