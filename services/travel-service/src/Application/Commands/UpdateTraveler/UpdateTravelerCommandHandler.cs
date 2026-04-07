using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateTraveler;

public sealed class UpdateTravelerCommandHandler(
    IBookingRepository bookingRepository,
    ITravelerRepository travelerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTravelerCommand>
{
    public async Task Handle(UpdateTravelerCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new DomainException($"Booking {request.BookingId} not found.");

        if (booking.TenantId != request.TenantId)
            throw new DomainException("Booking does not belong to the active tenant.");

        var traveler = await travelerRepository.GetByIdAsync(request.BookingId, request.TravelerId, cancellationToken)
            ?? throw new DomainException("Traveler not found.");

        if (traveler.TenantId != request.TenantId)
            throw new DomainException("Traveler does not belong to the active tenant.");

        traveler.Update(
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

        await travelerRepository.UpdateAsync(traveler, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
