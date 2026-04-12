using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.FollowUps;

public sealed class ReassignFollowUpCommandHandler(IFollowUpRepository followUpRepository, IFeatureGate featureGate, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork, Api.ITenantContext tenantContext) : IRequestHandler<ReassignFollowUpCommand>
{
    public async Task Handle(ReassignFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUp = await followUpRepository.GetByIdAsync(request.FollowUpId, cancellationToken)
            ?? throw new DomainException($"Follow-up {request.FollowUpId} not found.");

        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, followUp.TenantId, tenantContext.UserId, cancellationToken);

        followUp.Update(followUp.Subject, followUp.Notes, followUp.Priority, followUp.DueDate, request.AssignedToUserId);
        await followUpRepository.UpdateAsync(followUp, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(followUp.TenantId, "FollowUp", followUp.Id, "Updated", $"Follow-up reassigned: {followUp.Subject}", new { followUp.AssignedToUserId }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
