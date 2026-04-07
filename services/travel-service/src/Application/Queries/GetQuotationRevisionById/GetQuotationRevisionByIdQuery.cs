using MediatR;
using TravelService.Application.Queries.QuotationRevisions;

namespace TravelService.Application.Queries.GetQuotationRevisionById;

public sealed record GetQuotationRevisionByIdQuery(Guid TenantId, Guid QuotationId, Guid RevisionId) : IRequest<QuotationRevisionReadModel?>;
