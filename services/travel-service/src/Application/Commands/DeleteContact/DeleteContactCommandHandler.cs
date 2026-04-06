using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.DeleteContact;

public sealed class DeleteContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteContactCommand>
{
    public async Task Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Contact not found.");

        contact.SoftDelete();
        await contactRepository.UpdateAsync(contact, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
