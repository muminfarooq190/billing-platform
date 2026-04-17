using MediatR;

namespace TravelService.Application.Queries.DraftTripConcepts;

public sealed record GetDraftTripConceptByIdQuery(Guid TenantId, Guid InquiryId, Guid ConceptId) : IRequest<DraftTripConceptDetailReadModel?>;
