using MediatR;

namespace TravelService.Application.Commands.DraftTripConcepts;

public sealed record MarkPrimaryDraftTripConceptCommand(Guid TenantId, Guid InquiryId, Guid ConceptId) : IRequest;
