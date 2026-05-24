using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BillingService.Infrastructure.Persistence;

namespace BillingService.Application.Commands.SyncStripeSubscription;

public sealed class SyncStripeSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    BillingDbContext dbContext,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<SyncStripeSubscriptionCommandHandler> logger) : IRequestHandler<SyncStripeSubscriptionCommand, string>
{
    public async Task<string> Handle(SyncStripeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = request.LocalSubscriptionId.HasValue
            ? await subscriptionRepository.GetByIdAsync(request.LocalSubscriptionId.Value, cancellationToken)
            : null;

        // Fallback lookup by Stripe id when metadata.subscriptionId is missing
        // (events triggered from the Stripe dashboard manually).
        subscription ??= await dbContext.Subscriptions
            .SingleOrDefaultAsync(x => x.StripeSubscriptionId == request.StripeSubscriptionId && x.DeletedAt == null, cancellationToken);

        if (subscription is null)
        {
            logger.LogWarning("Stripe subscription {StripeId} arrived but no local Subscription matched.", request.StripeSubscriptionId);
            return "Unknown";
        }

        var eventType = request.EventType?.ToLowerInvariant() ?? string.Empty;
        var statusLower = request.Status?.ToLowerInvariant() ?? string.Empty;

        switch (eventType)
        {
            case "customer.subscription.created":
            case "customer.subscription.updated":
                if (request.CurrentPeriodStart is { } start && request.CurrentPeriodEnd is { } end)
                {
                    subscription.LinkStripeSubscription(request.StripeSubscriptionId, start, end);
                }
                // Stripe status mirrors our domain: active / past_due / canceled.
                if (statusLower == "past_due" || statusLower == "unpaid")
                {
                    subscription.MarkPastDue(Guid.Empty, DateTimeOffset.UtcNow, daysOverdue: 0);
                }
                else if (statusLower == "active" || statusLower == "trialing")
                {
                    // No-op — LinkStripeSubscription leaves status as-is; if we
                    // were previously PastDue and Stripe says active again,
                    // promote back.
                    if (subscription.Status == Domain.Enums.SubscriptionStatus.PastDue)
                    {
                        subscription.Reactivate();
                    }
                }
                break;

            case "customer.subscription.deleted":
                subscription.Cancel();
                break;

            case "customer.subscription.paused":
                // Paused = collection paused but access continues. Mirror.
                if (subscription.Status == Domain.Enums.SubscriptionStatus.Active)
                {
                    // Aggregate has no Pause method today; closest analog is
                    // PastDue. Log so we can add a proper Pause transition.
                    logger.LogInformation("Stripe paused subscription {StripeId} — local aggregate has no Pause transition; leaving status unchanged.", request.StripeSubscriptionId);
                }
                break;

            case "customer.subscription.resumed":
                if (subscription.Status != Domain.Enums.SubscriptionStatus.Active)
                    subscription.Reactivate();
                break;

            default:
                return "Ignored";
        }

        await subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync($"billing:dashboard:{subscription.TenantId}", cancellationToken);
        return "Synced";
    }
}
