using Dapper;
using MediatR;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetBookingById;

namespace TravelService.Application.Queries.ListBookings;

public sealed class ListBookingsQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListBookingsQuery, IReadOnlyList<BookingReadModel>>
{
    public async Task<IReadOnlyList<BookingReadModel>> Handle(ListBookingsQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var items = await dbConnection.QueryAsync<BookingReadModel>(
            @"SELECT id,
                      tenant_id AS TenantId,
                      quotation_id AS QuotationId,
                      accepted_revision_id AS AcceptedRevisionId,
                      primary_contact_id AS PrimaryContactId,
                      booking_number AS BookingNumber,
                      status,
                      trip_name AS TripName,
                      destination,
                      start_date AS StartDate,
                      end_date AS EndDate,
                      travellers_count AS TravellersCount,
                      currency,
                      total_sell_amount AS TotalSellAmount,
                      total_cost_amount AS TotalCostAmount,
                      margin_amount AS MarginAmount,
                      assigned_to_user_id AS AssignedToUserId,
                      customer_reference AS CustomerReference,
                      internal_notes AS InternalNotes,
                      created_at AS CreatedAt,
                      updated_at AS UpdatedAt,
                      cancelled_at AS CancelledAt
               FROM bookings
               WHERE tenant_id = @TenantId AND deleted_at IS NULL
               ORDER BY created_at DESC",
            new { request.TenantId });
        return items.ToList();
    }
}
