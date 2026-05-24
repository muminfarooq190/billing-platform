using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace BillingService.Infrastructure.Jobs;

/// <summary>
/// Daily subscription lifecycle scanner.
///
/// Walks every Active / PastDue / Cancelled subscription and emits the
/// appropriate domain events:
///
///   - Active w/ CurrentPeriodEnd ≤ 7 days   → SubscriptionExpiringSoonEvent
///                                              (tier picked from {7, 3, 1}).
///   - Cancelled w/ CurrentPeriodEnd ≤ now   → SubscriptionExpiredEvent
///                                              (once per subscription).
///   - Active w/ Overdue invoice ≥ grace     → MarkPastDue → SubscriptionPastDueEvent.
///
/// Idempotency for ExpiringSoon is best-effort via an in-process set keyed
/// on (SubscriptionId, tier, dateOnly) — the worker resets each day. For
/// stronger guarantees, add a `subscription_lifecycle_notifications` side
/// table; intentionally deferred so we don't ship a migration in this pass.
/// Expired + PastDue use aggregate-level idempotency (state transitions are
/// no-ops on second call).
///
/// Runs daily at startup + every 24h. Single-instance assumption — multiple
/// pods would double-emit ExpiringSoon notifications.
/// </summary>
public sealed class SubscriptionLifecycleWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionLifecycleWorker> logger) : BackgroundService
{
    private static readonly int[] WarningTiersDays = [7, 3, 1];
    private static readonly TimeSpan PastDueGrace = TimeSpan.FromDays(7);
    private static readonly TimeSpan PollInterval = TimeSpan.FromHours(24);

    /// <summary>
    /// (subscriptionId, tier, dateOnly-of-fire) → suppress double fires on
    /// the same day. Reset by an in-memory check; size bounded by tenant
    /// count × 3 tiers, so memory is trivial.
    /// </summary>
    private readonly HashSet<(Guid, int, DateOnly)> _firedExpiringSoonToday = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Subscription lifecycle scan failed");
            }
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var subRepo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
        var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        // -- 1) Expiring-soon: Active subscriptions w/ period ending in next 7 days.
        var expiringWindow = await subRepo.ListWithPeriodEndingBetweenAsync(
            now,
            now.AddDays(WarningTiersDays.Max()),
            [SubscriptionStatus.Active],
            cancellationToken);

        foreach (var sub in expiringWindow)
        {
            var daysLeft = (int)Math.Ceiling((sub.CurrentPeriodEnd - now).TotalDays);
            var tier = WarningTiersDays.Where(t => daysLeft <= t).Max(); // 7, 3, or 1
            var key = (sub.Id, tier, today);
            if (_firedExpiringSoonToday.Contains(key)) continue;

            sub.AddExpiringSoonNotification(daysLeft);
            await subRepo.UpdateAsync(sub, cancellationToken);
            _firedExpiringSoonToday.Add(key);
            logger.LogInformation("Subscription {Id} expiring in {Days} days (tier={Tier})", sub.Id, daysLeft, tier);
        }

        // -- 2) Expired: Cancelled subscriptions whose period has now elapsed.
        var elapsedCancelled = await subRepo.ListWithPeriodEndingBetweenAsync(
            DateTimeOffset.MinValue,
            now,
            [SubscriptionStatus.Cancelled],
            cancellationToken);

        foreach (var sub in elapsedCancelled.Where(s => s.IsPeriodElapsed(now)))
        {
            sub.MarkExpired();
            await subRepo.UpdateAsync(sub, cancellationToken);
            logger.LogInformation("Subscription {Id} marked expired (period elapsed {End})", sub.Id, sub.CurrentPeriodEnd);
        }

        // -- 3) Past-due: Active subscriptions w/ an Overdue invoice past grace.
        var overdueCandidates = await invoiceRepo.ListOverdueCandidatesAsync(now, cancellationToken);
        foreach (var invoice in overdueCandidates)
        {
            // Only flip subscription when the invoice has been past due for
            // ≥ grace window. ListOverdueCandidatesAsync returns anything past
            // DueDate; layer the grace check here.
            var daysOverdue = (int)Math.Floor((now - invoice.DueDate).TotalDays);
            if (daysOverdue < PastDueGrace.TotalDays) continue;

            var sub = await subRepo.GetByIdAsync(invoice.SubscriptionId, cancellationToken);
            if (sub is null || sub.Status != SubscriptionStatus.Active) continue;

            sub.MarkPastDue(invoice.Id, invoice.DueDate, daysOverdue);
            await subRepo.UpdateAsync(sub, cancellationToken);
            logger.LogInformation("Subscription {SubId} marked PastDue from invoice {InvId} ({Days} days late)", sub.Id, invoice.Id, daysOverdue);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Cleanup the in-memory fired set so tomorrow re-fires cleanly. We
        // only need to keep entries whose `date` matches today.
        _firedExpiringSoonToday.RemoveWhere(k => k.Item3 != today);
    }
}
