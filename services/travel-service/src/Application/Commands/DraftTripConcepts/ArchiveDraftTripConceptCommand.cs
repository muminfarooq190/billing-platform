using MediatR;

namespace TravelService.Application.Commands.DraftTripConcepts;

public sealed record ArchiveDraftTripConceptCommand(Guid TenantId, Guid InquiryId, Guid ConceptId) : IRequest;
