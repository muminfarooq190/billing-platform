using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Aggregates;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.Repositories;
using MediatR;

namespace CommunicationService.Application.Commands.SendNotification;

public sealed class SendNotificationCommandHandler(
    INotificationRepository notificationRepository,
    INotificationTemplateRepository templateRepository,
    IRecipientPreferencesRepository preferencesRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var recipientType = Enum.Parse<RecipientType>(request.RecipientType, true);
        var priority = Enum.Parse<NotificationPriority>(request.Priority, true);
        var placeholders = request.Placeholders ?? [];

        string subject;
        string body;
        Guid? templateId = null;
        ChannelType channel;

        if (!string.IsNullOrWhiteSpace(request.TemplateName))
        {
            var template = await templateRepository.GetByNameAndTenantAsync(request.TemplateName, request.TenantId, cancellationToken)
                ?? throw new DomainException($"Template '{request.TemplateName}' not found for tenant.");
            if (template.Status != TemplateStatus.Active)
                throw new DomainException($"Template '{request.TemplateName}' is not active.");

            subject = template.RenderSubject(placeholders);
            body = template.RenderBody(placeholders);
            templateId = template.Id;
            channel = template.Channel;
        }
        else
        {
            subject = request.Subject ?? throw new DomainException("Subject is required when not using a template.");
            body = request.Body ?? throw new DomainException("Body is required when not using a template.");
            channel = Enum.Parse<ChannelType>(request.Channel ?? "Email", true);
        }

        var preferences = await preferencesRepository.GetByRecipientIdAsync(request.RecipientId, request.TenantId, cancellationToken);
        if (preferences is not null && !preferences.IsChannelEnabled(channel) && priority != NotificationPriority.Critical)
        {
            throw new DomainException($"Recipient has disabled {channel} notifications. Use Critical priority to override.");
        }

        var notification = Notification.Create(
            request.TenantId,
            request.RecipientId,
            recipientType,
            channel,
            subject,
            body,
            priority,
            templateId,
            request.ReferenceId);

        notification.MarkQueued();
        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return notification.Id;
    }
}
