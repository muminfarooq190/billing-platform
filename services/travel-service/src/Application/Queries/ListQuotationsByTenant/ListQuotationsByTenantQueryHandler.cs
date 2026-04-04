using Dapper;
using TravelService.Application.Abstractions;
using TravelService.Application.Queries.GetQuotationById;
using MediatR;

namespace TravelService.Application.Queries.ListQuotationsByTenant;

public sealed class ListQuotationsByTenantQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<ListQuotationsByTenantQuery, IReadOnlyList<QuotationReadModel>>
{
    public async Task<IReadOnlyList<QuotationReadModel>> Handle(ListQuotationsByTenantQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        var results = await dbConnection.QueryAsync<QuotationReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, travel_date AS TravelDate, return_date AS ReturnDate, travellers, currency, notes, status, valid_until AS ValidUntil, total_amount AS TotalAmount, created_at AS CreatedAt, updated_at AS UpdatedAt FROM quotations WHERE tenant_id = @TenantId AND deleted_at IS NULL ORDER BY created_at DESC",
            new { request.TenantId });
        return results.ToList().AsReadOnly();
    }
}
