using MediatR;

namespace CommunicationService.Application.Commands.SendNotification;

public sealed record SendNotificationCommand(
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string? Channel,
    string? TemplateName,
    string? Subject,
    string? Body,
    string Priority,
    string? ReferenceId,
    Dictionary<string, string>? Placeholders) : IRequest<Guid>;
