using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.AddTraveler;

public sealed class AddTravelerCommandHandler(
    IBookingRepository bookingRepository,
    ITravelerRepository travelerRepository,
    IActivityWriter activityWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<AddTravelerCommand, Guid>
{
    public async Task<Guid> Handle(AddTravelerCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var traveler = Traveler.Create(
            request.BookingId,
            request.TenantId,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.Email,
            request.Phone,
            request.PassportNumber,
            request.PassportExpiry,
            request.Nationality,
            request.MealPreference,
            request.SpecialAssistanceNotes,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            request.LeadTraveler);

        await travelerRepository.AddAsync(traveler, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Booking",
                booking.Id,
                "TravelerAdded",
                $"Traveler added: {traveler.FirstName} {traveler.LastName}",
                new { traveler.Id, traveler.Email, traveler.LeadTraveler },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return traveler.Id;
    }
}
