using MediatR;

namespace TravelService.Application.Queries.ListTravelersByBooking;

public sealed record ListTravelersByBookingQuery(Guid TenantId, Guid BookingId) : IRequest<IReadOnlyList<TravelerReadModel>>;

public sealed record TravelerReadModel(
    Guid Id,
    Guid BookingId,
    Guid TenantId,
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
    bool LeadTraveler,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
