using TravelService.Application.Abstractions;
using TravelService.Domain.Aggregates;
using TravelService.Domain.Enums;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.CreateItinerary;

public sealed class CreateItineraryCommandHandler(IItineraryRepository itineraryRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateItineraryCommand, Guid>
{
    public async Task<Guid> Handle(CreateItineraryCommand request, CancellationToken cancellationToken)
    {
        var itinerary = Itinerary.Create(
            request.TenantId,
            request.CustomerContactId,
            request.CustomerName,
            request.Title,
            request.Destination,
            request.StartDate,
            request.EndDate,
            request.Travellers,
            request.Currency,
            request.QuotationId);

        foreach (var item in request.Items)
        {
            itinerary.AddItem(
                item.DayNumber,
                Enum.Parse<ItineraryItemType>(item.ItemType, true),
                item.Title,
                item.Description,
                item.Location,
                item.StartTime,
                item.EndTime,
                item.Cost,
                item.Currency);
        }

        await itineraryRepository.AddAsync(itinerary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return itinerary.Id;
    }
}
