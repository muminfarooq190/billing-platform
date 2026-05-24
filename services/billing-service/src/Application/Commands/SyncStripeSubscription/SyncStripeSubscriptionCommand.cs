using MediatR;

namespace BillingService.Application.Commands.SyncStripeSubscription;

/// <summary>
/// Sync a Stripe Subscription's lifecycle event into our Subscription
/// aggregate. Fired by the webhook controller for any
/// <c>customer.subscription.*</c> event:
///
///   created / updated  → link stripe_subscription_id + refresh period
///   deleted            → flip Status to Cancelled (Stripe terminated it)
///   paused / resumed   → flip Status to Paused / Active
///
/// Resolves our local Subscription by metadata.subscriptionId (set when
/// the checkout session was created) — falls back to stripe_subscription_id
/// lookup for events triggered outside our checkout flow.
/// </summary>
public sealed record SyncStripeSubscriptionCommand(
    string EventType,
    string StripeSubscriptionId,
    Guid? LocalSubscriptionId,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    string? Status) : IRequest<string>;
