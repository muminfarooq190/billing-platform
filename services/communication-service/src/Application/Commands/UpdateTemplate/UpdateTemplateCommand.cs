using MediatR;

namespace CommunicationService.Application.Commands.UpdateTemplate;

public sealed record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string Subject,
    string BodyTemplate,
    string Description,
    string? Action) : IRequest;
