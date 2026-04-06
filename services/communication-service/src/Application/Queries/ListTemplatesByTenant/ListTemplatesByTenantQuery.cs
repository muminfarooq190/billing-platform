using CommunicationService.Application.Queries.GetTemplateById;
using MediatR;

namespace CommunicationService.Application.Queries.ListTemplatesByTenant;

public sealed record ListTemplatesByTenantQuery(Guid TenantId, int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<TemplateReadModel>>;
