using MediatR;

namespace TravelService.Application.Commands.DraftTripConcepts;

public sealed record CreateDraftTripConceptCommand(
    Guid TenantId,
    Guid InquiryId,
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
    Guid? CreatedByUserId,
    List<CreateDraftTripConceptDayDto> Days) : IRequest<Guid>;

public sealed record CreateDraftTripConceptDayDto(
    int DayNumber,
    string Title,
    string? Description,
    string? Location,
    string? OvernightLocation);
