using BillingService.Application.Commands.GenerateInvoice;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Infrastructure.Jobs;

public sealed class BillingSchedulerService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var due = await repo.ListDueSubscriptionsAsync(DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        foreach (var subscription in due)
        {
            await mediator.Send(new GenerateInvoiceCommand(subscription.Id), cancellationToken);
            subscription.RenewNextCycle();
            await repo.UpdateAsync(subscription, cancellationToken);
        }
    }
}
