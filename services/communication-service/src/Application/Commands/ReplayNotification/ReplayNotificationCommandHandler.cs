using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.Repositories;
using MediatR;

namespace CommunicationService.Application.Commands.ReplayNotification;

public sealed class ReplayNotificationCommandHandler(INotificationRepository notificationRepository, IFeatureGate featureGate, IUnitOfWork unitOfWork, Api.ITenantContext tenantContext) : IRequestHandler<ReplayNotificationCommand>
{
    public async Task Handle(ReplayNotificationCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.CommunicationLogsRead, request.TenantId, tenantContext.UserId, cancellationToken);

        var notification = await notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new DomainException($"Notification {request.NotificationId} not found.");

        if (notification.TenantId != request.TenantId)
            throw new DomainException("Notification does not belong to the authenticated tenant.");

        if (notification.Status != NotificationStatus.Failed)
            throw new DomainException("Only failed notifications can be replayed.");

        notification.ResetForRetry();
        await notificationRepository.UpdateAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
