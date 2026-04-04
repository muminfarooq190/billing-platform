using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetItineraryById;
using MediatR;

namespace TravelService.Application.Queries.ListItinerariesByTenant;

public sealed class ListItinerariesByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListItinerariesByTenantQuery, IReadOnlyList<ItineraryReadModel>>
{
    public async Task<IReadOnlyList<ItineraryReadModel>> Handle(ListItinerariesByTenantQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<ItineraryReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, start_date AS StartDate, end_date AS EndDate, travellers, currency, quotation_id AS QuotationId, status, COALESCE((SELECT SUM(cost) FROM itinerary_items WHERE itinerary_id = itineraries.id), 0) AS TotalCost, created_at AS CreatedAt, updated_at AS UpdatedAt FROM itineraries WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY start_date",
            new { request.TenantId });
        return results.ToList().AsReadOnly();
    }
}
