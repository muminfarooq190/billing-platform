using MediatR;

namespace CommunicationService.Application.Commands.ReplayNotification;

public sealed record ReplayNotificationCommand(Guid TenantId, Guid NotificationId, string? Reason) : IRequest;