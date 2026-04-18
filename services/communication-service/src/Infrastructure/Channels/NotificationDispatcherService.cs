using CommunicationService.Application.Abstractions;
using CommunicationService.Domain.Enums;
using CommunicationService.Domain.Repositories;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Recipients;

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
                var recipientAddressResolver = scope.ServiceProvider.GetRequiredService<IRecipientAddressResolver>();
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
                    var recipient = await recipientAddressResolver.ResolveAsync(notification, preferences, stoppingToken);
                    if (string.IsNullOrWhiteSpace(recipient))
                    {
                        notification.MarkFailed($"Could not resolve recipient address for recipient {notification.RecipientId} and channel {notification.Channel}.");
                        await notificationRepo.UpdateAsync(notification, stoppingToken);
                        continue;
                    }

                    var result = await dispatcher.SendAsync(recipient, notification.Subject, notification.Body, stoppingToken);

                    if (result.Success)
                        notification.MarkSent(result.ProviderMessageId);
                    else
                        notification.MarkFailed(result.ErrorMessage ?? "Unknown error");

                    await notificationRepo.UpdateAsync(notification, stoppingToken);
                }

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
