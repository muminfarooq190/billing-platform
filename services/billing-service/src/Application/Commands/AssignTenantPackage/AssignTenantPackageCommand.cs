using MediatR;

namespace BillingService.Application.Commands.AssignTenantPackage;

public sealed record AssignTenantPackageCommand(
    Guid TenantId,
    Guid CommercialPackageId,
    string Source,
    string Status,
    DateTimeOffset EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    string? MetadataJson) : IRequest<Guid>;
