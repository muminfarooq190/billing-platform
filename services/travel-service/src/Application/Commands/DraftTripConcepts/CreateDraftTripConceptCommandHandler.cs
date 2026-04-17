using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using TravelService.Application.Commands.TravelInquiries;

namespace TravelService.Application.Commands.DraftTripConcepts;

public sealed class CreateDraftTripConceptCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IDraftTripConceptRepository conceptRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateDraftTripConceptCommand, Guid>
{
    public async Task<Guid> Handle(CreateDraftTripConceptCommand request, CancellationToken cancellationToken)
    {
        _ = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);

        var concept = DraftTripConcept.Create(
            request.TenantId,
            request.InquiryId,
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
            request.CreatedByUserId);

        foreach (var day in request.Days)
            concept.AddDay(day.DayNumber, day.Title, day.Description, day.Location, day.OvernightLocation);

        await conceptRepository.AddAsync(concept, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelInquiry",
                request.InquiryId,
                "DraftTripConceptCreated",
                $"Draft trip concept '{concept.Title}' created",
                new { ConceptId = concept.Id, concept.Destination, concept.IsPrimary, concept.ConceptStatus },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "DraftTripConcept",
                concept.Id,
                "DraftTripConceptCreated",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { concept.Title, concept.Destination, concept.OptionLabel, concept.ConceptStatus },
                new { request.InquiryId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return concept.Id;
    }
}
