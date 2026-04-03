using MediatR;

namespace BillingService.Application.Commands.CancelSubscription;

public sealed record CancelSubscriptionCommand(Guid SubscriptionId) : IRequest;
