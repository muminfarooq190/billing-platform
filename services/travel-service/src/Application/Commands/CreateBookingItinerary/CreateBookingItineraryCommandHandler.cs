using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.CreateItinerary;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.CreateBookingItinerary;

public sealed class CreateBookingItineraryCommandHandler(
    IBookingRepository bookingRepository,
    IItineraryRepository itineraryRepository,
    IActivityWriter activityWriter,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateBookingItineraryCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingItineraryCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var itinerary = Itinerary.Create(
            booking.TenantId,
            booking.PrimaryContactId,
            booking.TripName,
            request.Title,
            request.Destination,
            request.StartDate,
            request.EndDate,
            request.Travellers,
            request.Currency,
            booking.QuotationId,
            booking.Id);

        foreach (var item in request.Items)
        {
            itinerary.AddItem(
                item.DayNumber,
                Enum.Parse<ItineraryItemType>(item.ItemType, true),
                item.Title,
                item.Description,
                item.Location,
                item.StartTime,
                item.EndTime,
                item.Cost,
                item.Currency);
        }

        await itineraryRepository.AddAsync(itinerary, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                booking.TenantId,
                "Booking",
                booking.Id,
                "ItineraryCreated",
                $"Itinerary {itinerary.Id} created for booking {booking.BookingNumber}",
                new { ItineraryId = itinerary.Id, booking.QuotationId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return itinerary.Id;
    }
}
