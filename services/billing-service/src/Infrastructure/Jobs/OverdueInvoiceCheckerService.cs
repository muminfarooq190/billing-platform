using BillingService.Application.Commands.MarkInvoiceOverdue;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Infrastructure.Jobs;

public sealed class OverdueInvoiceCheckerService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunOnceAsync(stoppingToken);
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var overdue = await repo.ListOverdueCandidatesAsync(DateTimeOffset.UtcNow, cancellationToken);
        foreach (var invoice in overdue)
        {
            await mediator.Send(new MarkInvoiceOverdueCommand(invoice.Id), cancellationToken);
        }
    }
}
