using BillingService.Application.ReadModels;
using MediatR;

namespace BillingService.Application.Commands.GrantFeatureEntitlement;

public sealed record GrantFeatureEntitlementCommand(
    Guid TenantId,
    string FeatureKey,
    bool Granted,
    int? LimitValue,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? Reason) : IRequest<FeatureEntitlementReadModel>;
