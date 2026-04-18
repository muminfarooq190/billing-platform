using MediatR;

namespace CommunicationService.Application.Queries.GetNotificationDetail;

public sealed record GetNotificationDetailQuery(Guid TenantId, Guid NotificationId) : IRequest<NotificationDetailReadModel?>;

public sealed record NotificationDetailReadModel(
    Guid Id,
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Channel,
    string Subject,
    string Body,
    string Priority,
    string Status,
    string ReferenceId,
    string CorrelationId,
    string WorkflowType,
    string? IdempotencyKey,
    string DocumentReferencesJson,
    string MetadataJson,
    int RetryCount,
    string? LastError,
    string? ProviderMessageId,
    DateTimeOffset? SentAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
