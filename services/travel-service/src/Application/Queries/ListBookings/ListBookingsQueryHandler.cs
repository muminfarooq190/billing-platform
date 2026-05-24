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
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);
        var offset = (page - 1) * pageSize;

        // Bug fix: TotalPaidAmount used to be hardcoded `0::numeric` and
        // TotalOutstandingAmount equaled total_sell_amount, so the bookings
        // list always showed "Paid $0 / Outstanding $TotalSell" even when
        // payments were recorded via booking_payments. We now project from
        // the same source the per-booking financial-summary endpoint uses:
        // sum of Paid + Refunded payments. Refunded reduces the paid total
        // so outstanding swings back up correctly.
        var items = await dbConnection.QueryAsync<BookingReadModel>(
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
                      COALESCE(p.paid_amount, 0)::numeric AS TotalPaidAmount,
                      GREATEST(b.total_sell_amount - COALESCE(p.paid_amount, 0), 0)::numeric AS TotalOutstandingAmount,
                      b.total_cost_amount AS TotalCostAmount,
                      b.margin_amount AS MarginAmount,
                      b.assigned_to_user_id AS AssignedToUserId,
                      b.customer_reference AS CustomerReference,
                      b.internal_notes AS InternalNotes,
                      c.first_name || CASE WHEN c.last_name IS NOT NULL AND c.last_name <> '' THEN ' ' || c.last_name ELSE '' END AS CustomerName,
                      b.travellers_count AS TravelersRequired,
                      COALESCE(t.complete_count, 0) AS TravelersComplete,
                      b.travellers_count AS DocumentsRequired,
                      COALESCE(d.uploaded_count, 0) AS DocumentsUploaded,
                      i.id AS ItineraryId,
                      CASE WHEN i.id IS NOT NULL THEN TRUE ELSE FALSE END AS HasItinerary,
                      i.status AS ItineraryStatus,
                      i.updated_at AS ItineraryUpdatedAt,
                      b.created_at AS CreatedAt,
                      b.updated_at AS UpdatedAt,
                      b.cancelled_at AS CancelledAt
               FROM bookings b
               LEFT JOIN contacts c ON c.id = b.primary_contact_id AND c.deleted_at IS NULL
               LEFT JOIN LATERAL (
                    SELECT COUNT(*)::int AS uploaded_count
                    FROM booking_documents bd
                    WHERE bd.booking_id = b.id AND bd.deleted_at IS NULL
               ) d ON TRUE
               LEFT JOIN LATERAL (
                    SELECT COUNT(*) FILTER (
                        WHERE t.passport_number IS NOT NULL
                          AND t.passport_expiry IS NOT NULL
                          AND t.nationality IS NOT NULL
                    )::int AS complete_count
                    FROM travelers t
                    WHERE t.booking_id = b.id AND t.deleted_at IS NULL
               ) t ON TRUE
               LEFT JOIN LATERAL (
                    SELECT COALESCE(SUM(CASE WHEN status = 'Paid' THEN amount
                                             WHEN status = 'Refunded' THEN -amount
                                             ELSE 0 END), 0) AS paid_amount
                    FROM booking_payments
                    WHERE booking_id = b.id AND deleted_at IS NULL
               ) p ON TRUE
               LEFT JOIN LATERAL (
                    SELECT id, status, updated_at
                    FROM itineraries
                    WHERE booking_id = b.id AND deleted_at IS NULL
                    ORDER BY updated_at DESC NULLS LAST
                    LIMIT 1
               ) i ON TRUE
               WHERE b.tenant_id = @TenantId
                 AND b.deleted_at IS NULL
                 AND (@Status IS NULL OR b.status = @Status)
                 AND (@Destination IS NULL OR b.destination ILIKE '%' || @Destination || '%')
                 AND (@StartDateFrom IS NULL OR b.start_date >= @StartDateFrom)
                 AND (@StartDateTo IS NULL OR b.start_date <= @StartDateTo)
                 AND (@AssignedToUserId IS NULL OR b.assigned_to_user_id = @AssignedToUserId)
                 AND (@PrimaryContactId IS NULL OR b.primary_contact_id = @PrimaryContactId)
               ORDER BY b.created_at DESC
               LIMIT @PageSize OFFSET @Offset",
            new { request.TenantId, request.Status, request.Destination, request.StartDateFrom, request.StartDateTo, request.AssignedToUserId, request.PrimaryContactId, PageSize = pageSize, Offset = offset });
        return items.ToList();
    }
}
