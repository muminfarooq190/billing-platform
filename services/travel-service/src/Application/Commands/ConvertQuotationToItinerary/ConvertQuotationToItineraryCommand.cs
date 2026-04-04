using MediatR;

namespace TravelService.Application.Commands.ConvertQuotationToItinerary;

public sealed record ConvertQuotationToItineraryCommand(Guid QuotationId) : IRequest<Guid>;
