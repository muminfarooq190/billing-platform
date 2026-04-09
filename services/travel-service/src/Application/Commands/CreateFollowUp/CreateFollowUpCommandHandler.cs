using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.CreateFollowUp;

public sealed class CreateFollowUpCommandHandler(IFollowUpRepository followUpRepository, IActivityWriter activityWriter, IActorContext actorContext, IUnitOfWork unitOfWork) : IRequestHandler<CreateFollowUpCommand, Guid>
{
    public async Task<Guid> Handle(CreateFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUp = FollowUp.Create(
            request.TenantId,
            request.CustomerContactId,
            request.CustomerName,
            request.Subject,
            request.Notes,
            Enum.Parse<FollowUpPriority>(request.Priority, true),
            request.DueDate,
            request.AssignedToUserId);
        await followUpRepository.AddAsync(followUp, cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "FollowUp",
                followUp.Id,
                "Created",
                $"Follow-up created: {followUp.Subject}",
                new { followUp.CustomerContactId, followUp.Priority, followUp.DueDate, followUp.Status },
                actorContext.UserId),
            cancellationToken);
        await activityWriter.WriteAsync(
            ActivityEntry.Create(
                request.TenantId,
                "Contact",
                followUp.CustomerContactId,
                "Created",
                $"Follow-up created for {followUp.CustomerName}",
                new { FollowUpId = followUp.Id, followUp.Subject, followUp.Priority, followUp.DueDate, followUp.Status },
                actorContext.UserId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return followUp.Id;
    }
}
