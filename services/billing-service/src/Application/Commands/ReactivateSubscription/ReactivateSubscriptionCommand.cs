using MediatR;

namespace BillingService.Application.Commands.ReactivateSubscription;

/// <summary>
/// Flip a Cancelled or PastDue subscription back to Active. Throws
/// `DomainException` when the period has already elapsed — the caller
/// should create a fresh subscription instead.
/// </summary>
public sealed record ReactivateSubscriptionCommand(Guid SubscriptionId) : IRequest;
