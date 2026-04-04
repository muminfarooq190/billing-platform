using TravelService.Application.Abstractions;
using TravelService.Domain.Exceptions;
using TravelService.Domain.Repositories;
using MediatR;

namespace TravelService.Application.Commands.UpdateItinerary;

public sealed class UpdateItineraryCommandHandler(IItineraryRepository itineraryRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateItineraryCommand>
{
    public async Task Handle(UpdateItineraryCommand request, CancellationToken cancellationToken)
    {
        var itinerary = await itineraryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Itinerary {request.Id} not found.");

        switch (request.Action?.ToLowerInvariant())
        {
            case "confirm": itinerary.Confirm(); break;
            case "start": itinerary.Start(); break;
            case "complete": itinerary.Complete(); break;
            case "cancel": itinerary.Cancel(); break;
            default:
                itinerary.Update(request.Title, request.Destination, request.StartDate, request.EndDate, request.Travellers, request.Currency);
                break;
        }

        await itineraryRepository.UpdateAsync(itinerary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
