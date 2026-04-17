using Dapper;
using TravelService.Application.Abstractions;
using MediatR;

namespace TravelService.Application.Queries.GetItineraryById;

public sealed class GetItineraryByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetItineraryByIdQuery, ItineraryReadModel?>
{
    public async Task<ItineraryReadModel?> Handle(GetItineraryByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<ItineraryReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, start_date AS StartDate, end_date AS EndDate, travellers, currency, quotation_id AS QuotationId, booking_id AS BookingId, CASE WHEN booking_id IS NOT NULL THEN TRUE ELSE FALSE END AS IsBookingOwned, CASE WHEN booking_id IS NOT NULL THEN 'Booking' WHEN quotation_id IS NOT NULL THEN 'QuotationLegacy' ELSE 'StandaloneLegacy' END AS OwnershipType, status, COALESCE((SELECT SUM(cost) FROM itinerary_items WHERE itinerary_id = itineraries.id), 0) AS TotalCost, created_at AS CreatedAt, updated_at AS UpdatedAt FROM itineraries WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
