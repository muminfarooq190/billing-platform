using BillingService.Domain.Enums;

namespace BillingService.Api.Contracts;

public sealed record UpsertCommercialPackageRequest(
    string Code,
    string Name,
    string Category,
    string BillingModel,
    string Description,
    bool IsActive,
    string? MetadataJson);

public sealed record CommercialPackageFeatureRequest(
    string FeatureKey,
    bool Granted,
    int? LimitValue,
    LimitMergePolicy LimitMergePolicy,
    string? MetadataJson);

public sealed record ReplaceCommercialPackageFeaturesRequest(
    IReadOnlyList<CommercialPackageFeatureRequest> Features);
