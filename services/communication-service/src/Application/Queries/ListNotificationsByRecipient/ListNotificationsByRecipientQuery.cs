using MediatR;

namespace CommunicationService.Application.Queries.ListNotificationsByRecipient;

public sealed record ListNotificationsByRecipientQuery(Guid RecipientId) : IRequest<IReadOnlyList<NotificationReadModel>>;

public sealed record NotificationReadModel(
    Guid Id,
    Guid TenantId,
    Guid RecipientId,
    string RecipientType,
    string Channel,
    string Subject,
    string Priority,
    string Status,
    int RetryCount,
    string? LastError,
    string? ProviderMessageId,
    DateTimeOffset? SentAt,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset CreatedAt);
