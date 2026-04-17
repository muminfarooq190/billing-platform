using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetItineraryById;
using MediatR;
using System.Text;

namespace TravelService.Application.Queries.ListItinerariesByTenant;

public sealed class ListItinerariesByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListItinerariesByTenantQuery, IReadOnlyList<ItineraryReadModel>>
{
    public async Task<IReadOnlyList<ItineraryReadModel>> Handle(ListItinerariesByTenantQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;

        var sql = new StringBuilder("SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, start_date AS StartDate, end_date AS EndDate, travellers, currency, quotation_id AS QuotationId, booking_id AS BookingId, CASE WHEN booking_id IS NOT NULL THEN TRUE ELSE FALSE END AS IsBookingOwned, CASE WHEN booking_id IS NOT NULL THEN 'Booking' WHEN quotation_id IS NOT NULL THEN 'QuotationLegacy' ELSE 'StandaloneLegacy' END AS OwnershipType, status, COALESCE((SELECT SUM(cost) FROM itinerary_items WHERE itinerary_id = itineraries.id), 0) AS TotalCost, created_at AS CreatedAt, updated_at AS UpdatedAt FROM itineraries WHERE tenant_id = @TenantId AND deleted_at IS NULL");

        if (!string.IsNullOrWhiteSpace(request.Status))
            sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            sql.Append(" AND customer_name ILIKE @CustomerName");
        if (request.StartDateFrom.HasValue)
            sql.Append(" AND start_date >= @StartDateFrom");
        if (request.StartDateTo.HasValue)
            sql.Append(" AND start_date <= @StartDateTo");
        if (request.BookingId.HasValue)
            sql.Append(" AND booking_id = @BookingId");
        if (request.QuotationId.HasValue)
            sql.Append(" AND quotation_id = @QuotationId");
        if (!string.IsNullOrWhiteSpace(request.OwnershipType))
        {
            if (string.Equals(request.OwnershipType, "Booking", StringComparison.OrdinalIgnoreCase))
                sql.Append(" AND booking_id IS NOT NULL");
            else if (string.Equals(request.OwnershipType, "QuotationLegacy", StringComparison.OrdinalIgnoreCase))
                sql.Append(" AND booking_id IS NULL AND quotation_id IS NOT NULL");
            else if (string.Equals(request.OwnershipType, "StandaloneLegacy", StringComparison.OrdinalIgnoreCase))
                sql.Append(" AND booking_id IS NULL AND quotation_id IS NULL");
        }

        sql.Append(" ORDER BY CASE WHEN booking_id IS NOT NULL THEN 0 ELSE 1 END, start_date OFFSET @Offset LIMIT @Limit");

        var results = await dbConnection.QueryAsync<ItineraryReadModel>(sql.ToString(), new
        {
            request.TenantId,
            request.Status,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : $"%{request.CustomerName.Trim()}%",
            request.StartDateFrom,
            request.StartDateTo,
            request.BookingId,
            request.QuotationId,
            request.OwnershipType,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return results.ToList().AsReadOnly();
    }
}
