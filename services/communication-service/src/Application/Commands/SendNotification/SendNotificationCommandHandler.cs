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
    IFeatureGate featureGate,
    IBrandingTemplateRenderer brandingTemplateRenderer,
    IChannelPreferenceResolver channelPreferenceResolver,
    IUnitOfWork unitOfWork,
    Api.ITenantContext tenantContext) : IRequestHandler<SendNotificationCommand, Guid>
{
    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationNotificationSend, request.TenantId, tenantContext.UserId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await notificationRepository.GetByIdempotencyKeyAsync(request.TenantId, request.IdempotencyKey, cancellationToken);
            if (existing is not null)
                return existing.Id;
        }

        var recipientType = Enum.Parse<RecipientType>(request.RecipientType, true);
        var priority = Enum.Parse<NotificationPriority>(request.Priority, true);
        var brandingChannel = string.IsNullOrWhiteSpace(request.Channel) ? "Email" : request.Channel!;
        var placeholders = await brandingTemplateRenderer.EnrichAsync(request.TenantId, brandingChannel, request.Placeholders ?? [], cancellationToken);

        var preferences = await preferencesRepository.GetByRecipientIdAsync(request.RecipientId, request.TenantId, cancellationToken);

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
            channel = string.IsNullOrWhiteSpace(request.Channel)
                ? channelPreferenceResolver.ResolvePreferredChannel(null, preferences, template.Channel)
                : template.Channel;
        }
        else
        {
            subject = request.Subject ?? throw new DomainException("Subject is required when not using a template.");
            body = request.Body ?? throw new DomainException("Body is required when not using a template.");
            channel = channelPreferenceResolver.ResolvePreferredChannel(request.Channel, preferences, ChannelType.Email);
        }

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
            request.ReferenceId,
            request.CorrelationId,
            request.WorkflowType,
            request.IdempotencyKey,
            request.DocumentReferencesJson,
            request.MetadataJson);

        notification.MarkQueued();
        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return notification.Id;
    }
}
