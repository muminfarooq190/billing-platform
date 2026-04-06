using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;

namespace TravelService.Application.Commands.CreateContact;

public sealed class CreateContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateContactCommand, Guid>
{
    public async Task<Guid> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = Contact.Create(
            request.TenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Company,
            request.Notes,
            request.Tags);

        await contactRepository.AddAsync(contact, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return contact.Id;
    }
}
