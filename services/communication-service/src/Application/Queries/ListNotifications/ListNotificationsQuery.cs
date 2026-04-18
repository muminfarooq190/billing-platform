using MediatR;

namespace CommunicationService.Application.Queries.ListNotifications;

public sealed record ListNotificationsQuery(
    Guid TenantId,
    string? Status,
    string? Channel,
    string? ReferenceId,
    string? CorrelationId,
    string? WorkflowType,
    Guid? RecipientId,
    int Page = 1,
    int PageSize = 20) : IRequest<IReadOnlyList<NotificationListItemReadModel>>;

public sealed record NotificationListItemReadModel(
    Guid Id,
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Channel,
    string Subject,
    string Priority,
    string Status,
    string ReferenceId,
    string CorrelationId,
    string WorkflowType,
    string? IdempotencyKey,
    int RetryCount,
    string? LastError,
    string? ProviderMessageId,
    DateTimeOffset? SentAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);
