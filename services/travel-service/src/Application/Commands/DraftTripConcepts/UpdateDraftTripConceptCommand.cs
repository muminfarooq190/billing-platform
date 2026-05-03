using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.DraftTripConcepts;

public sealed record UpdateDraftTripConceptDayDto(int DayNumber, string Title, string? Description, string? Location, string? OvernightLocation);

public sealed record UpdateDraftTripConceptCommand(
    Guid TenantId,
    Guid InquiryId,
    Guid ConceptId,
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
    IReadOnlyCollection<UpdateDraftTripConceptDayDto> Days) : IRequest;

public sealed class UpdateDraftTripConceptCommandHandler(IDraftTripConceptRepository conceptRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateDraftTripConceptCommand>
{
    public async Task Handle(UpdateDraftTripConceptCommand request, CancellationToken cancellationToken)
    {
        var concept = await conceptRepository.GetByIdAsync(request.ConceptId, cancellationToken);
        if (concept is null || concept.TenantId != request.TenantId || concept.TravelInquiryId != request.InquiryId)
            throw new InvalidOperationException("Concept not found.");

        concept.Update(
            request.Title,
            request.Destination,
            request.Summary,
            request.StartDate,
            request.EndDate,
            request.Travellers,
            request.Currency,
            request.BudgetAmount,
            request.OptionLabel,
            request.Notes,
            request.Days.Select(x => (x.DayNumber, x.Title, x.Description, x.Location, x.OvernightLocation)).ToList());

        await conceptRepository.UpdateAsync(concept, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
