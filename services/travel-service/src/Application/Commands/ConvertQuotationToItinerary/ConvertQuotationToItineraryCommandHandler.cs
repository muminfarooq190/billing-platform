using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.ConvertQuotationToItinerary;

// Legacy compatibility flow only.
// Preferred lifecycle is: accepted quotation -> booking -> booking-owned confirmed itinerary.
public sealed class ConvertQuotationToItineraryCommandHandler(
    IQuotationRepository quotationRepository,
    IItineraryRepository itineraryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConvertQuotationToItineraryCommand, Guid>
{
    public async Task<Guid> Handle(ConvertQuotationToItineraryCommand request, CancellationToken cancellationToken)
    {
        var quotation = await quotationRepository.GetByIdAsync(request.QuotationId, cancellationToken)
            ?? throw new DomainException($"Quotation {request.QuotationId} not found.");

        quotation.MarkConverted();

        var itinerary = Itinerary.Create(
            quotation.TenantId,
            quotation.CustomerContactId,
            quotation.CustomerName,
            quotation.Title,
            quotation.Destination,
            quotation.TravelDate,
            quotation.ReturnDate,
            quotation.Travellers,
            quotation.Currency,
            quotation.Id);

        foreach (var lineItem in quotation.LineItems)
        {
            itinerary.AddItem(
                1,
                ItineraryItemType.Other,
                lineItem.Description,
                lineItem.Description,
                quotation.Destination,
                null,
                null,
                lineItem.Total,
                lineItem.Currency);
        }

        await quotationRepository.UpdateAsync(quotation, cancellationToken);
        await itineraryRepository.AddAsync(itinerary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return itinerary.Id;
    }
}
