using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;

namespace CommunicationService.Infrastructure.Channels;

public sealed class InAppDispatcher(ILogger<InAppDispatcher> logger) : IChannelDispatcher
{
    public ChannelType Channel => ChannelType.InApp;

    public async Task<ChannelDispatchResult> SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken)
    {
        // In-app notifications are stored in the DB and read via API — mark as delivered immediately
        logger.LogInformation("IN-APP for recipient={RecipientId} subject={Subject}", recipient, subject);
        await Task.CompletedTask;
        return new ChannelDispatchResult(true, $"inapp-{Guid.NewGuid():N}", null);
    }
}
