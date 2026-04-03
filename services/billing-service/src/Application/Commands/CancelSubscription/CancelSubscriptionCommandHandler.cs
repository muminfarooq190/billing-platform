using BillingService.Application.Abstractions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.CancelSubscription;

public sealed class CancelSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork) : IRequestHandler<CancelSubscriptionCommand>
{
    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByIdAsync(request.SubscriptionId, cancellationToken) ?? throw new InvalidOperationException("Subscription not found.");
        subscription.Cancel();
        await subscriptionRepository.UpdateAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
