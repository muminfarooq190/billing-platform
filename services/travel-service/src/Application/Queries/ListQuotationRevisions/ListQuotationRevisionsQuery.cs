using MediatR;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Application.Queries.ListQuotationRevisions;

public sealed record ListQuotationRevisionsQuery(Guid TenantId, Guid QuotationId) : IRequest<IReadOnlyList<QuotationRevisionSummaryReadModel>>;
