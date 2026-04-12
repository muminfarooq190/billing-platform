using BillingService.Application.Abstractions;
using BillingService.Domain.Exceptions;
using BillingService.Domain.Repositories;
using MediatR;

namespace BillingService.Application.Commands.RevokeUserFeatureAssignment;

public sealed class RevokeUserFeatureAssignmentCommandHandler(
    ITenantUserFeatureAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokeUserFeatureAssignmentCommand>
{
    public async Task Handle(RevokeUserFeatureAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetActiveAssignmentAsync(request.TenantId, request.UserId, request.FeatureKey, cancellationToken)
            ?? throw new DomainException("Active assignment not found.");

        assignment.Revoke(request.RevokedByUserId, DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
