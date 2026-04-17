using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.GetBookingById;

public sealed class GetBookingByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetBookingByIdQuery, BookingReadModel?>
{
    public async Task<BookingReadModel?> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<BookingReadModel>(
            @"SELECT b.id,
                      b.tenant_id AS TenantId,
                      b.quotation_id AS QuotationId,
                      b.accepted_revision_id AS AcceptedRevisionId,
                      b.primary_contact_id AS PrimaryContactId,
                      b.booking_number AS BookingNumber,
                      b.status,
                      b.trip_name AS TripName,
                      b.destination,
                      b.start_date AS StartDate,
                      b.end_date AS EndDate,
                      b.travellers_count AS TravellersCount,
                      b.currency,
                      b.total_sell_amount AS TotalSellAmount,
                      b.total_cost_amount AS TotalCostAmount,
                      b.margin_amount AS MarginAmount,
                      b.assigned_to_user_id AS AssignedToUserId,
                      b.customer_reference AS CustomerReference,
                      b.internal_notes AS InternalNotes,
                      i.id AS ItineraryId,
                      i.status AS ItineraryStatus,
                      i.updated_at AS ItineraryUpdatedAt,
                      b.created_at AS CreatedAt,
                      b.updated_at AS UpdatedAt,
                      b.cancelled_at AS CancelledAt
               FROM bookings b
               LEFT JOIN itineraries i ON i.booking_id = b.id AND i.deleted_at IS NULL
               WHERE b.id = @BookingId AND b.tenant_id = @TenantId AND b.deleted_at IS NULL
               ORDER BY i.updated_at DESC NULLS LAST
               LIMIT 1",
            new { request.BookingId, request.TenantId });
    }
}
