namespace TravelService.Api.Contracts;

public sealed record AddTravelerRequest(
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
    bool LeadTraveler);

public sealed record UpdateTravelerRequest(
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
    bool LeadTraveler);
