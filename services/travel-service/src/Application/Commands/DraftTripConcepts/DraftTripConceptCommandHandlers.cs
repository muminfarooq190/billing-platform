using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Commands.TravelInquiries;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.DraftTripConcepts;

internal static class DraftTripConceptCommandSupport
{
    public static async Task<DraftTripConcept> LoadConceptAsync(IDraftTripConceptRepository repository, Guid tenantId, Guid inquiryId, Guid conceptId, CancellationToken cancellationToken)
    {
        var concept = await repository.GetByIdAsync(conceptId, cancellationToken)
            ?? throw new DomainException($"Draft trip concept {conceptId} not found.");

        if (concept.TenantId != tenantId || concept.TravelInquiryId != inquiryId)
            throw new DomainException("Draft trip concept does not belong to tenant inquiry context.");

        return concept;
    }
}

public sealed class MarkPrimaryDraftTripConceptCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IDraftTripConceptRepository conceptRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkPrimaryDraftTripConceptCommand>
{
    public async Task Handle(MarkPrimaryDraftTripConceptCommand request, CancellationToken cancellationToken)
    {
        _ = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var concepts = await conceptRepository.ListByInquiryIdAsync(request.InquiryId, cancellationToken);
        var target = concepts.SingleOrDefault(x => x.Id == request.ConceptId)
            ?? throw new DomainException($"Draft trip concept {request.ConceptId} not found.");

        foreach (var concept in concepts)
        {
            if (concept.Id == target.Id)
                concept.MarkPrimary();
            else
                concept.ClearPrimary();

            await conceptRepository.UpdateAsync(concept, cancellationToken);
        }

        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelInquiry",
                request.InquiryId,
                "DraftTripConceptMarkedPrimary",
                $"Draft trip concept '{target.Title}' marked primary",
                new { ConceptId = target.Id },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "DraftTripConcept",
                target.Id,
                "DraftTripConceptMarkedPrimary",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { target.IsPrimary },
                new { request.InquiryId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ArchiveDraftTripConceptCommandHandler(
    ITravelInquiryRepository inquiryRepository,
    IDraftTripConceptRepository conceptRepository,
    IActivityWriter activityWriter,
    IAuditWriter auditWriter,
    IActorContext actorContext,
    IUnitOfWork unitOfWork) : IRequestHandler<ArchiveDraftTripConceptCommand>
{
    public async Task Handle(ArchiveDraftTripConceptCommand request, CancellationToken cancellationToken)
    {
        _ = await TravelInquiryCommandHandlerSupport.LoadInquiryAsync(inquiryRepository, request.TenantId, request.InquiryId, cancellationToken);
        var concept = await DraftTripConceptCommandSupport.LoadConceptAsync(conceptRepository, request.TenantId, request.InquiryId, request.ConceptId, cancellationToken);

        concept.Archive();
        await conceptRepository.UpdateAsync(concept, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "TravelInquiry",
                request.InquiryId,
                "DraftTripConceptArchived",
                $"Draft trip concept '{concept.Title}' archived",
                new { ConceptId = concept.Id },
                actorContext.UserId),
            cancellationToken);
        await auditWriter.WriteAsync(
            AuditLog.Create(
                request.TenantId,
                "DraftTripConcept",
                concept.Id,
                "DraftTripConceptArchived",
                actorContext.UserId,
                actorContext.IpAddress,
                actorContext.UserAgent,
                null,
                new { concept.ConceptStatus },
                new { request.InquiryId }),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
