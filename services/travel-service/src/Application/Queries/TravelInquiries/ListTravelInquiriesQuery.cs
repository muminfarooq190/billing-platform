using MediatR;

namespace TravelService.Application.Queries.TravelInquiries;

public sealed record ListTravelInquiriesQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? Source = null,
    Guid? AssignedToUserId = null,
    string? Destination = null,
    string? Query = null) : IRequest<IReadOnlyList<TravelInquiryListItemReadModel>>;
