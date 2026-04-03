using MediatR;

namespace BillingService.Application.Commands.CreateSubscription;

public sealed record CreateSubscriptionCommand(Guid TenantId, string PlanType, string BillingCycle) : IRequest<Guid>;
