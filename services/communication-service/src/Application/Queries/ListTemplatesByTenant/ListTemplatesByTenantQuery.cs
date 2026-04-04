using CommunicationService.Application.Queries.GetTemplateById;
using MediatR;

namespace CommunicationService.Application.Queries.ListTemplatesByTenant;

public sealed record ListTemplatesByTenantQuery(Guid TenantId) : IRequest<IReadOnlyList<TemplateReadModel>>;
