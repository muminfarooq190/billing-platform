using MediatR;

namespace CommunicationService.Application.Commands.SendWorkflowNotification;

public sealed record SendWorkflowNotificationCommand(
    Guid TenantId,
    string WorkflowType,
    Guid RecipientId,
    string RecipientType,
    string? Channel,
    string? TemplateName,
    string? Subject,
    string? Body,
    string? Priority,
    string? ReferenceId,
    string? CorrelationId,
    string? IdempotencyKey,
    string DocumentReferencesJson,
    string MetadataJson,
    Dictionary<string, string>? Placeholders) : IRequest<Guid>;
