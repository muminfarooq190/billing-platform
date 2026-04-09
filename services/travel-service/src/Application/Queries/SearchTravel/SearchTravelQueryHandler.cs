using Dapper;
using MediatR;
using TravelService.Application.Abstractions;

namespace TravelService.Application.Queries.SearchTravel;

public sealed class SearchTravelQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<SearchTravelQuery, IReadOnlyList<SearchResultReadModel>>
{
    public async Task<IReadOnlyList<SearchResultReadModel>> Handle(SearchTravelQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var q = $"%{request.Query}%";

        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SearchResultReadModel>(@"
            SELECT 'Contact' AS EntityType, id AS EntityId, concat(first_name, ' ', last_name) AS Title, coalesce(email, phone, company, '') AS Subtitle, created_at AS RelevantDate
            FROM contacts
            WHERE tenant_id = @TenantId AND deleted_at IS NULL AND (first_name ILIKE @Query OR last_name ILIKE @Query OR email ILIKE @Query OR company ILIKE @Query)
            UNION ALL
            SELECT 'Quotation' AS EntityType, id AS EntityId, title AS Title, destination AS Subtitle, travel_date AS RelevantDate
            FROM quotations
            WHERE tenant_id = @TenantId AND deleted_at IS NULL AND (title ILIKE @Query OR destination ILIKE @Query OR customer_name ILIKE @Query)
            UNION ALL
            SELECT 'Booking' AS EntityType, id AS EntityId, booking_number AS Title, destination AS Subtitle, travel_date AS RelevantDate
            FROM bookings
            WHERE tenant_id = @TenantId AND deleted_at IS NULL AND (booking_number ILIKE @Query OR destination ILIKE @Query OR title ILIKE @Query)
            UNION ALL
            SELECT 'Traveler' AS EntityType, id AS EntityId, concat(first_name, ' ', last_name) AS Title, coalesce(email, phone, nationality, '') AS Subtitle, created_at AS RelevantDate
            FROM travelers
            WHERE tenant_id = @TenantId AND deleted_at IS NULL AND (first_name ILIKE @Query OR last_name ILIKE @Query OR email ILIKE @Query OR passport_number ILIKE @Query)
            ORDER BY RelevantDate DESC NULLS LAST
            OFFSET @Offset LIMIT @Limit", new
        {
            request.TenantId,
            Query = q,
            Offset = (page - 1) * pageSize,
            Limit = pageSize
        });

        return rows.ToList();
    }
}
