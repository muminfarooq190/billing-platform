using BillingService.Application.Abstractions;
using BillingService.Domain.Exceptions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.ReactivateSubscription;

public sealed class ReactivateSubscriptionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ReactivateSubscriptionCommand>
{
    public async Task Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        // Reactivation only valid while the original period is still in
        // force — once elapsed, the user must create a new subscription.
        // This mirrors the portal "cancelled-ending" → expired transition.
        if (subscription.IsPeriodElapsed(DateTimeOffset.UtcNow))
            throw new DomainException("Subscription period has already elapsed. Create a new subscription instead of reactivating.");

        subscription.Reactivate();
        await subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
