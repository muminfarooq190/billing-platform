using MediatR;

namespace CommunicationService.Application.Queries.ListNotificationsByRecipient;

public sealed record ListNotificationsByRecipientQuery(Guid TenantId, Guid RecipientId, int Page = 1, int PageSize = 20) : IRequest<IReadOnlyList<NotificationReadModel>>;

public sealed record NotificationReadModel(
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
