using MediatR;

namespace CommunicationService.Application.Queries.GetTemplateById;

public sealed record GetTemplateByIdQuery(Guid Id) : IRequest<TemplateReadModel?>;

public sealed record TemplateReadModel(
    Guid Id,
    Guid TenantId,
    string Name,
    string Subject,
    string BodyTemplate,
    string Channel,
    string Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
