using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.FollowUps;

public sealed class CompleteFollowUpCommandHandler(IFollowUpRepository followUpRepository, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork) : IRequestHandler<CompleteFollowUpCommand>
{
    public async Task Handle(CompleteFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUp = await followUpRepository.GetByIdAsync(request.FollowUpId, cancellationToken)
            ?? throw new DomainException($"Follow-up {request.FollowUpId} not found.");

        followUp.MarkCompleted();
        await followUpRepository.UpdateAsync(followUp, cancellationToken);
        await activityWriter.WriteAsync(ActivityEntry.Create(followUp.TenantId, "FollowUp", followUp.Id, "StatusChanged", $"Follow-up completed: {followUp.Subject}", new { followUp.Status, followUp.DueDate, followUp.AssignedToUserId }, actorContext.UserId), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
