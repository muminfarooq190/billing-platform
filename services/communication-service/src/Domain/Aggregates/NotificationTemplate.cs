using CommunicationService.Domain.Common;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Events;
using CommunicationService.Domain.Exceptions;

namespace CommunicationService.Domain.Aggregates;

public sealed class NotificationTemplate : AggregateRoot
{
    private NotificationTemplate() { }

    private NotificationTemplate(Guid tenantId, string name, string subject, string bodyTemplate, ChannelType channel, string? description)
    {
        if (tenantId == Guid.Empty) throw new DomainException("Tenant id is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Template name is required.");
        if (string.IsNullOrWhiteSpace(subject)) throw new DomainException("Template subject is required.");
        if (string.IsNullOrWhiteSpace(bodyTemplate)) throw new DomainException("Template body is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Name = name.Trim();
        Subject = subject.Trim();
        BodyTemplate = bodyTemplate.Trim();
        Channel = channel;
        Description = description?.Trim() ?? string.Empty;
        Status = TemplateStatus.Draft;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TemplateCreatedEvent(Id, TenantId, Name));
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public ChannelType Channel { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public TemplateStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public static NotificationTemplate Create(Guid tenantId, string name, string subject, string bodyTemplate, ChannelType channel, string? description)
        => new(tenantId, name, subject, bodyTemplate, channel, description);

    public void Update(string name, string subject, string bodyTemplate, string description)
    {
        if (Status == TemplateStatus.Archived)
            throw new DomainException("Cannot update an archived template.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required.");
        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Template subject is required.");
        if (string.IsNullOrWhiteSpace(bodyTemplate))
            throw new DomainException("Template body is required.");

        Name = name.Trim();
        Subject = subject.Trim();
        BodyTemplate = bodyTemplate.Trim();
        Description = description?.Trim() ?? string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status == TemplateStatus.Archived)
            throw new DomainException("Cannot activate an archived template.");
        Status = TemplateStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        Status = TemplateStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string RenderBody(Dictionary<string, string> placeholders)
    {
        var body = BodyTemplate;
        foreach (var (key, value) in placeholders)
        {
            body = body.Replace($"{{{{{key}}}}}", value);
        }
        return body;
    }

    public string RenderSubject(Dictionary<string, string> placeholders)
    {
        var subject = Subject;
        foreach (var (key, value) in placeholders)
        {
            subject = subject.Replace($"{{{{{key}}}}}", value);
        }
        return subject;
    }
}
