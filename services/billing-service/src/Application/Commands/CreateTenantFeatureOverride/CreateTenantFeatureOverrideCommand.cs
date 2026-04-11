using MediatR;

namespace BillingService.Application.Commands.CreateTenantFeatureOverride;

public sealed record CreateTenantFeatureOverrideCommand(
    Guid TenantId,
    string FeatureKey,
    bool Granted,
    int? LimitValue,
    string Reason,
    string Source,
    string? CreatedBy,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson) : IRequest<Guid>;
