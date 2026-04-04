using TravelService.Application.Abstractions;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.UpdateFollowUp;

public sealed class UpdateFollowUpCommandHandler(IFollowUpRepository followUpRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateFollowUpCommand>
{
    public async Task Handle(UpdateFollowUpCommand request, CancellationToken cancellationToken)
    {
        var followUp = await followUpRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Follow-up {request.Id} not found.");

        followUp.Update(request.Subject, request.Notes, Enum.Parse<FollowUpPriority>(request.Priority, true), request.DueDate, request.AssignedToUserId);

        var status = Enum.Parse<FollowUpStatus>(request.Status, true);
        switch (status)
        {
            case FollowUpStatus.InProgress: followUp.MarkInProgress(); break;
            case FollowUpStatus.Completed: followUp.Complete(); break;
            case FollowUpStatus.Cancelled: followUp.Cancel(); break;
        }

        await followUpRepository.UpdateAsync(followUp, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
