using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetItineraryById;

namespace TravelService.Application.Queries.GetBookingItinerary;

public sealed class GetBookingItineraryQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetBookingItineraryQuery, ItineraryReadModel?>
{
    public async Task<ItineraryReadModel?> Handle(GetBookingItineraryQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        return await dbConnection.QuerySingleOrDefaultAsync<ItineraryReadModel>(@"
SELECT i.id,
       i.tenant_id AS TenantId,
       i.customer_contact_id AS CustomerContactId,
       i.customer_name AS CustomerName,
       i.title,
       i.destination,
       i.start_date AS StartDate,
       i.end_date AS EndDate,
       i.travellers,
       i.currency,
       i.quotation_id AS QuotationId,
       i.booking_id AS BookingId,
       CASE WHEN i.booking_id IS NOT NULL THEN TRUE ELSE FALSE END AS IsBookingOwned,
       CASE WHEN i.booking_id IS NOT NULL THEN 'Booking' WHEN i.quotation_id IS NOT NULL THEN 'QuotationLegacy' ELSE 'StandaloneLegacy' END AS OwnershipType,
       i.status,
       COALESCE((SELECT SUM(cost) FROM itinerary_items WHERE itinerary_id = i.id), 0) AS TotalCost,
       i.created_at AS CreatedAt,
       i.updated_at AS UpdatedAt
FROM itineraries i
INNER JOIN bookings b ON b.id = i.booking_id AND b.tenant_id = @TenantId AND b.deleted_at IS NULL
WHERE i.booking_id = @BookingId AND i.deleted_at IS NULL
ORDER BY i.updated_at DESC NULLS LAST
LIMIT 1", new { request.TenantId, request.BookingId });
    }
}
