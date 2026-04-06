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

        var sql = new StringBuilder("SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, start_date AS StartDate, end_date AS EndDate, travellers, currency, quotation_id AS QuotationId, status, COALESCE((SELECT SUM(cost) FROM itinerary_items WHERE itinerary_id = itineraries.id), 0) AS TotalCost, created_at AS CreatedAt, updated_at AS UpdatedAt FROM itineraries WHERE tenant_id = @TenantId AND deleted_at IS NULL");

        if (!string.IsNullOrWhiteSpace(request.Status))
            sql.Append(" AND status = @Status");
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            sql.Append(" AND customer_name ILIKE @CustomerName");
        if (request.StartDateFrom.HasValue)
            sql.Append(" AND start_date >= @StartDateFrom");
        if (request.StartDateTo.HasValue)
            sql.Append(" AND start_date <= @StartDateTo");

        sql.Append(" ORDER BY start_date OFFSET @Offset LIMIT @Limit");

        var results = await dbConnection.QueryAsync<ItineraryReadModel>(sql.ToString(), new
        {
            request.TenantId,
            request.Status,
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName) ? null : $"%{request.CustomerName.Trim()}%",
            request.StartDateFrom,
            request.StartDateTo,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return results.ToList().AsReadOnly();
    }
}
