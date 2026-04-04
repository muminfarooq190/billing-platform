using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.CreateFollowUp;

public sealed class CreateFollowUpCommandHandler(IFollowUpRepository followUpRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateFollowUpCommand, Guid>
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
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return followUp.Id;
    }
}
