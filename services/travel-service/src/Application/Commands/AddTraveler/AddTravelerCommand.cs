using MediatR;

namespace TravelService.Application.Commands.AddTraveler;

public sealed record AddTravelerCommand(
    Guid TenantId,
    Guid BookingId,
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
    bool LeadTraveler) : IRequest<Guid>;
