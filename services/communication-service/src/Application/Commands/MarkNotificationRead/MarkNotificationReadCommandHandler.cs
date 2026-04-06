using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Exceptions;
using CommunicationService.Domain.Repositories;
using MediatR;

namespace CommunicationService.Application.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken)
            ?? throw new DomainException("Notification not found.");

        notification.MarkAsRead();
        await notificationRepository.UpdateAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
