namespace BillingService.Api.Contracts;

public sealed record AssignTenantPackageRequest(
    Guid CommercialPackageId,
    string Source,
    string Status,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson);
