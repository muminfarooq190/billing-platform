using MediatR;

namespace BillingService.Application.Commands.RevokeUserFeatureAssignment;

public sealed record RevokeUserFeatureAssignmentCommand(Guid TenantId, Guid UserId, string FeatureKey, Guid? RevokedByUserId) : IRequest;
