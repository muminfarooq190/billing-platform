using CommunicationService.Domain.Common;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Events;
using CommunicationService.Domain.Exceptions;

namespace CommunicationService.Domain.Aggregates;

public sealed class Notification : AggregateRoot
{
    private Notification() { }

    private Notification(Guid tenantId, Guid recipientId, RecipientType recipientType, ChannelType channel, string subject, string body, NotificationPriority priority, Guid? templateId, string? referenceId)
    {
        if (tenantId == Guid.Empty) throw new DomainException("Tenant id is required.");
        if (recipientId == Guid.Empty) throw new DomainException("Recipient id is required.");
        if (string.IsNullOrWhiteSpace(subject)) throw new DomainException("Subject is required.");
        if (string.IsNullOrWhiteSpace(body)) throw new DomainException("Body is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        RecipientId = recipientId;
        RecipientType = recipientType;
        Channel = channel;
        Subject = subject.Trim();
        Body = body.Trim();
        Priority = priority;
        TemplateId = templateId;
        ReferenceId = referenceId?.Trim() ?? string.Empty;
        Status = NotificationStatus.Pending;
        RetryCount = 0;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid RecipientId { get; private set; }
    public RecipientType RecipientType { get; private set; }
    public ChannelType Channel { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationPriority Priority { get; private set; }
    public NotificationStatus Status { get; private set; }
    public Guid? TemplateId { get; private set; }
    public string ReferenceId { get; private set; } = string.Empty;
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Notification Create(Guid tenantId, Guid recipientId, RecipientType recipientType, ChannelType channel, string subject, string body, NotificationPriority priority, Guid? templateId, string? referenceId)
        => new(tenantId, recipientId, recipientType, channel, subject, body, priority, templateId, referenceId);

    public void MarkQueued()
    {
        if (Status != NotificationStatus.Pending)
            throw new DomainException("Only pending notifications can be queued.");
        Status = NotificationStatus.Queued;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NotificationQueuedEvent(Id, TenantId, Channel.ToString()));
    }

    public void MarkSent(string? providerMessageId)
    {
        if (Status is not NotificationStatus.Queued and not NotificationStatus.Pending)
            throw new DomainException("Only pending or queued notifications can be marked sent.");

        Status = NotificationStatus.Sent;
        ProviderMessageId = providerMessageId?.Trim();
        LastError = null;
        SentAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NotificationSentEvent(Id, TenantId, Channel.ToString()));
    }

    public void MarkDelivered()
    {
        if (Status != NotificationStatus.Sent)
            throw new DomainException("Only sent notifications can be marked delivered.");

        Status = NotificationStatus.Delivered;
        DeliveredAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsRead()
    {
        if (ReadAt is not null)
            return;

        ReadAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason is required.");

        Status = NotificationStatus.Failed;
        LastError = reason.Trim();
        RetryCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NotificationFailedEvent(Id, TenantId, Channel.ToString(), LastError));
    }

    public void MarkBounced(string reason)
    {
        Status = NotificationStatus.Bounced;
        LastError = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool CanRetry(int maxRetries = 3) => Status == NotificationStatus.Failed && RetryCount < maxRetries;

    public void ResetForRetry()
    {
        if (!CanRetry())
            throw new DomainException("Notification has exceeded max retry attempts or is not in a retryable state.");

        Status = NotificationStatus.Queued;
        LastError = null;
        ProviderMessageId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new NotificationQueuedEvent(Id, TenantId, Channel.ToString()));
    }
}
