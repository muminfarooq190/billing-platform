using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.CreateQuotation;

public sealed class CreateQuotationCommandHandler(IQuotationRepository quotationRepository, IFeatureGate featureGate, IUnitOfWork unitOfWork) : IRequestHandler<CreateQuotationCommand, Guid>
{
    public async Task<Guid> Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
    {
        await featureGate.EnsureEnabledAsync(FeatureKeys.TravelQuotationCreate, request.TenantId, cancellationToken);

        var quotation = Quotation.Create(
            request.TenantId,
            request.CustomerContactId,
            request.CustomerName,
            request.Title,
            request.Destination,
            request.TravelDate,
            request.ReturnDate,
            request.Travellers,
            request.Currency,
            request.Notes);

        foreach (var item in request.LineItems)
        {
            quotation.AddLineItem(item.Description, item.UnitPrice, item.Quantity, item.Currency);
        }

        await quotationRepository.AddAsync(quotation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return quotation.Id;
    }
}
