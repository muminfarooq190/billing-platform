using MediatR;

namespace BillingService.Application.Commands.AssignUserFeatures;

public sealed record AssignUserFeaturesCommand(
    Guid TenantId,
    Guid UserId,
    IReadOnlyList<string> FeatureKeys,
    Guid? AssignedByUserId,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? Notes,
    string? MetadataJson) : IRequest<IReadOnlyList<string>>;
