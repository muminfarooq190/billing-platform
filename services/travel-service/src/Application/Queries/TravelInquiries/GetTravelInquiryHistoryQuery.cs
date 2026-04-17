using MediatR;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed record GetTravelInquiryHistoryQuery(Guid TenantId, Guid InquiryId) : IRequest<IReadOnlyList<TravelInquiryHistoryReadModel>>;
