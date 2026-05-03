namespace TravelService.Api.Contracts;

public sealed record UpdateDraftTripConceptRequest(
    string Title,
    string Destination,
    string? Summary,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    int? Travellers,
    string? Currency,
    decimal? BudgetAmount,
    string? OptionLabel,
    string? Notes,
    IReadOnlyList<UpdateDraftTripConceptDayRequest>? Days);

public sealed record UpdateDraftTripConceptDayRequest(
    int DayNumber,
    string Title,
    string? Description,
    string? Location,
    string? OvernightLocation);
