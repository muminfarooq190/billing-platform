using MediatR;

namespace TravelService.Application.Queries.DraftTripConcepts;

public sealed record ListDraftTripConceptsByInquiryQuery(Guid TenantId, Guid InquiryId) : IRequest<IReadOnlyList<DraftTripConceptListItemReadModel>>;
