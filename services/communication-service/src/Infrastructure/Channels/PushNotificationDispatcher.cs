using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Channels;

public sealed class PushNotificationDispatcher(ILogger<PushNotificationDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.PushNotification;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        logger.LogInformation("PUSH to={DeviceToken} title={Subject} body_length={BodyLength}", recipient, subject, body.Length);
        await Task.CompletedTask;
        // Future: Firebase Cloud Messaging / APNs integration
        return new ChannelDispatchResult(true, $"push-{Guid.NewGuid():N}", null);
    }
}
