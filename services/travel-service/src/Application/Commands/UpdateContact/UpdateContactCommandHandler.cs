using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.UpdateContact;

public sealed class UpdateContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateContactCommand>
{
    public async Task Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("Contact not found.");

        contact.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Company,
            request.Notes,
            request.Tags);

        await contactRepository.UpdateAsync(contact, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
