using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.UpdateQuotation;

public sealed class UpdateQuotationCommandHandler(IQuotationRepository quotationRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateQuotationCommand>
{
    public async Task Handle(UpdateQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Quotation {request.Id} not found.");

        switch (request.Action?.ToLowerInvariant())
        {
            case "send": quotation.Send(); break;
            case "accept": quotation.Accept(); break;
            case "reject": quotation.Reject(); break;
            default:
                quotation.Update(request.Title, request.Destination, request.TravelDate, request.ReturnDate, request.Travellers, request.Currency, request.Notes, request.ValidUntil);
                break;
        }

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
