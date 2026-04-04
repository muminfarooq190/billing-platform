using Dapper;
using TravelService.Application.Abstractions;
using MediatR;

namespace TravelService.Application.Queries.GetQuotationById;

public sealed class GetQuotationByIdQueryHandler(IReadDbConnectionFactory connectionFactory) : IRequestHandler<GetQuotationByIdQuery, QuotationReadModel?>
{
    public async Task<QuotationReadModel?> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken) as IAsyncDisposable;
        var dbConnection = (System.Data.IDbConnection)connection!;
        return await dbConnection.QuerySingleOrDefaultAsync<QuotationReadModel>(
            "SELECT id, tenant_id AS TenantId, customer_contact_id AS CustomerContactId, customer_name AS CustomerName, title, destination, travel_date AS TravelDate, return_date AS ReturnDate, travellers, currency, notes, status, valid_until AS ValidUntil, total_amount AS TotalAmount, created_at AS CreatedAt, updated_at AS UpdatedAt FROM quotations WHERE id = @Id AND deleted_at IS NULL",
            new { request.Id });
    }
}
