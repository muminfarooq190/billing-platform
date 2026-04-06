using TravelService.Application.Queries.GetQuotationById;
using MediatR;

namespace TravelService.Application.Queries.ListQuotationsByTenant;

public sealed record ListQuotationsByTenantQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Status = null,
    string? CustomerName = null,
    DateTimeOffset? TravelDateFrom = null,
    DateTimeOffset? TravelDateTo = null)
    : IRequest<IReadOnlyList<QuotationReadModel>>;
