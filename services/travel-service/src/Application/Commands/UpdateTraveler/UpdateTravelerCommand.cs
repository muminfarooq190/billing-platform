using MediatR;

namespace TravelService.Application.Commands.UpdateTraveler;

public sealed record UpdateTravelerCommand(
    Guid TenantId,
    Guid BookingId,
    Guid TravelerId,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? Gender,
    string? Email,
    string? Phone,
    string? PassportNumber,
    DateOnly? PassportExpiry,
    string? Nationality,
    string? MealPreference,
    string? SpecialAssistanceNotes,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    bool LeadTraveler) : IRequest;
