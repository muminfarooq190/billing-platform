using MediatR;
using TravelService.Application.Queries.GetItineraryById;

namespace TravelService.Application.Queries.GetBookingItinerary;

public sealed record GetBookingItineraryQuery(Guid TenantId, Guid BookingId) : IRequest<ItineraryReadModel?>;
