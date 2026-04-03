using BillingService.Application.Abstractions;
using BillingService.Domain.Aggregates;
using BillingService.Domain.Enums;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandHandler(ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateSubscriptionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = Subscription.Create(request.TenantId, Enum.Parse<PlanType>(request.PlanType, true), Enum.Parse<BillingCycle>(request.BillingCycle, true));
        await subscriptionRepository.AddAsync(subscription, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return subscription.Id;
    }
}
