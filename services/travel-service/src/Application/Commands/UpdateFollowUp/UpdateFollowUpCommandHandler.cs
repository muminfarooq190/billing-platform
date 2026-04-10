using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.UpdateFollowUp;

public sealed class UpdateFollowUpCommandHandler(IFollowUpRepository followUpRepository, IFeatureGate featureGate, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork) : IRequestHandler<UpdateFollowUpCommand>
{
    public async Task Handle(UpdateFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUp = await followUpRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Follow-up {request.Id} not found.");

        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelTimelineRead, followUp.TenantId, cancellationToken);

        var previousStatus = followUp.Status;
        followUp.Update(request.Subject, request.Notes, Enum.Parse<FollowUpPriority>(request.Priority, true), request.DueDate, request.AssignedToUserId);

        var status = Enum.Parse<FollowUpStatus>(request.Status, true);
        switch (status)
        {
            case FollowUpStatus.InProgress: followUp.MarkInProgress(); break;
            case FollowUpStatus.Completed: followUp.Complete(); break;
            case FollowUpStatus.Cancelled: followUp.Cancel(); break;
        }

        await followUpRepository.UpdateAsync(followUp, cancellationToken);
        var activityType = status == FollowUpStatus.Completed ? "StatusChanged" : "Updated";
        var summary = status == FollowUpStatus.Completed
            ? $"Follow-up completed: {followUp.Subject}"
            : $"Follow-up updated: {followUp.Subject}";
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                followUp.TenantId,
                "FollowUp",
                followUp.Id,
                activityType,
                summary,
                new { PreviousStatus = previousStatus.ToString(), Status = followUp.Status.ToString(), followUp.CustomerContactId, followUp.Priority, followUp.DueDate },
                actorContext.UserId),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                followUp.TenantId,
                "Contact",
                followUp.CustomerContactId,
                activityType,
                summary,
                new { FollowUpId = followUp.Id, PreviousStatus = previousStatus.ToString(), Status = followUp.Status.ToString(), followUp.Priority, followUp.DueDate },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
