using IdentityService.Application.Abstractions;
using IdentityService.Domain.Exceptions;
using IdentityService.Domain.Repositories;
using MediatR;

namespace IdentityService.Application.Commands.AnonymizeUser;

public sealed class AnonymizeUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork) : IRequestHandler<AnonymizeUserCommand>
{
    public async Task Handle(AnonymizeUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.Anonymize();
        await userRepository.UpdateAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
