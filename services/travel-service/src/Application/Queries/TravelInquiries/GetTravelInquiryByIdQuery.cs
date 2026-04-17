using MediatR;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed record GetTravelInquiryByIdQuery(Guid TenantId, Guid InquiryId) : IRequest<TravelInquiryDetailReadModel?>;
