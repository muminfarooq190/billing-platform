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
    string? CorrelationId,
    string? IdempotencyKey,
    string? WorkflowType,
    string DocumentReferencesJson,
    string MetadataJson,
    Dictionary<string, string>? Placeholders) : IRequest<Guid>;
