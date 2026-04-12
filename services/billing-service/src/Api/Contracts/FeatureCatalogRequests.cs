namespace BillingService.Api.Contracts;

public sealed record CreateFeatureCatalogEntryRequest(
    string FeatureKey,
    string Service,
    string Category,
    string DisplayName,
    string Description,
    bool IsQuota,
    string? Unit,
    string? MetadataJson,
    string? AssignmentMode,
    int? DefaultAssignmentLimit);

public sealed record UpdateFeatureCatalogEntryRequest(
    string Service,
    string Category,
    string DisplayName,
    string Description,
    bool IsQuota,
    string? Unit,
    string? MetadataJson,
    string? AssignmentMode,
    int? DefaultAssignmentLimit);
