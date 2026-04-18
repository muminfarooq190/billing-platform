using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.CreateBookingItinerary;

public sealed class CreateBookingItineraryCommandHandler(
    IBookingRepository bookingRepository,
    IItineraryRepository itineraryRepository,
    ICommunicationWorkflowClient communicationWorkflowClient,
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

        var publicBaseUrl = Environment.GetEnvironmentVariable("TRAVEL_PUBLIC_BASE_URL")?.TrimEnd('/') ?? "http://localhost:5060";
        var itineraryUrl = $"{publicBaseUrl}/travel/bookings/{booking.Id:D}/itinerary";
        await communicationWorkflowClient.SendItineraryAsync(request.TenantId, new ItineraryCommunicationRequest(
            booking.PrimaryContactId,
            "Email",
            $"Your itinerary is ready - {request.Title}",
            $"Your itinerary for {request.Destination} is ready. Please review the attached/shared itinerary document.",
            itinerary.Id.ToString("D"),
            booking.Id.ToString("D"),
            $"itinerary-sent:{itinerary.Id:D}",
            [
                new CommunicationDocumentReference(
                    $"itinerary-{itinerary.Id:D}.pdf",
                    itinerary.Id.ToString("D"),
                    itineraryUrl,
                    "application/pdf",
                    null,
                    new Dictionary<string, string> { ["bookingId"] = booking.Id.ToString("D"), ["quotationId"] = booking.QuotationId?.ToString("D") ?? string.Empty })
            ]), cancellationToken);

        return itinerary.Id;
    }
}
