using TravelService.Application.Queries.GetQuotationById;
using MediatR;

namespace TravelService.Application.Queries.ListQuotationsByTenant;

public sealed record ListQuotationsByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<QuotationReadModel>>;
