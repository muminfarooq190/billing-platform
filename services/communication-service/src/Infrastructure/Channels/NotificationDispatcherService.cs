using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Persistence;

namespace CommunicationService.Infrastructure.Channels;

public sealed class NotificationDispatcherService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationDispatcherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var preferencesRepo = scope.ServiceProvider.GetRequiredService<IRecipientPreferencesRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var dispatchers = scope.ServiceProvider.GetServices<IChannelDispatcher>().ToDictionary(d => d.Channel);

                var pending = await notificationRepo.ListPendingAsync(20, stoppingToken);
                foreach (var notification in pending)
                {
                    if (!dispatchers.TryGetValue(notification.Channel, out var dispatcher))
                    {
                        notification.MarkFailed($"No dispatcher registered for channel {notification.Channel}");
                        await notificationRepo.UpdateAsync(notification, stoppingToken);
                        continue;
                    }

                    var preferences = await preferencesRepo.GetByRecipientIdAsync(notification.RecipientId, notification.TenantId, stoppingToken);
                    var recipient = notification.Channel switch
                    {
                        ChannelType.Email => preferences?.Email ?? "unknown",
                        ChannelType.Sms => preferences?.Phone ?? "unknown",
                        ChannelType.PushNotification => preferences?.DeviceToken ?? "unknown",
                        _ => notification.RecipientId.ToString()
                    };

                    var result = await dispatcher.SendAsync(recipient, notification.Subject, notification.Body, stoppingToken);

                    if (result.Success)
                        notification.MarkSent(result.ProviderMessageId);
                    else
                        notification.MarkFailed(result.ErrorMessage ?? "Unknown error");

                    await notificationRepo.UpdateAsync(notification, stoppingToken);
                }

                // Retry failed notifications
                var retryable = await notificationRepo.ListRetryableAsync(3, 10, stoppingToken);
                foreach (var notification in retryable)
                {
                    notification.ResetForRetry();
                    await notificationRepo.UpdateAsync(notification, stoppingToken);
                }

                if (pending.Count > 0 || retryable.Count > 0)
                    await unitOfWork.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error in notification dispatcher loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
    }
}
