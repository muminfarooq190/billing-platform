using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateTraveler;

public sealed class UpdateTravelerCommandHandler(
    IBookingRepository bookingRepository,
    ITravelerRepository travelerRepository,
    IAuditWriter auditWriter,
    IActorContext actorContext,
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

        var before = new
        {
            traveler.FirstName,
            traveler.LastName,
            traveler.DateOfBirth,
            traveler.Gender,
            traveler.Email,
            traveler.Phone,
            traveler.PassportExpiry,
            traveler.Nationality,
            traveler.MealPreference,
            traveler.SpecialAssistanceNotes,
            traveler.EmergencyContactName,
            traveler.EmergencyContactPhone,
            traveler.LeadTraveler
        };

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
        await auditWriter.WriteAsync(
            AuditLog.Create(
                traveler.TenantId,
                "Traveler",
                traveler.Id,
                "Updated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                before: before,
                after: new
                {
                    traveler.FirstName,
                    traveler.LastName,
                    traveler.DateOfBirth,
                    traveler.Gender,
                    traveler.Email,
                    traveler.Phone,
                    traveler.PassportExpiry,
                    traveler.Nationality,
                    traveler.MealPreference,
                    traveler.SpecialAssistanceNotes,
                    traveler.EmergencyContactName,
                    traveler.EmergencyContactPhone,
                    traveler.LeadTraveler
                },
                metadata: new { traveler.BookingId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
