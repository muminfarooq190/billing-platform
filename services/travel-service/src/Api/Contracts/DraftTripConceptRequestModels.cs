namespace TravelService.Api.Contracts;

public sealed record DraftTripConceptDayRequest(
    int DayNumber,
    string Title,
    string? Description,
    string? Location,
    string? OvernightLocation);

public sealed record CreateDraftTripConceptRequest(
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
    List<DraftTripConceptDayRequest>? Days);
