using MediatR;

namespace CommunicationService.Application.Commands.CreateTemplate;

public sealed record CreateTemplateCommand(
    Guid TenantId,
    string Name,
    string Subject,
    string BodyTemplate,
    string Channel,
    string? Description) : IRequest<Guid>;
